using System;
using System.Collections.Generic;
using System.IO;

namespace Metran.FileSystem.Fat.VFATLayer
{
    public class DirectoryEntryManager : IDirectoryEntryManager
    {
        private const string DotEntryName = ".";
        private const string DotEntryExtension = "";
        private const DirectoryEntryAttributes DotEntryAttributes = DirectoryEntryAttributes.Directory;

        private const string DotdotEntryName = "..";
        private const string DotdotEntryExtension = "";
        private const DirectoryEntryAttributes DotdotEntryAttributes = DirectoryEntryAttributes.Directory;

        private static readonly char[] ShortNameSpecialChars =
        {
            '$', '%', '\'', '-', '_', '@', '~', '`', '!', '(', ')',
            '{', '}', '^', '#', '&'
        };

        private static readonly char[] LongNameSpecialChars = {'+', ',', '.', ';', '=', '[', ']'};

        private static bool IsAllowedForShortName(char c)
        {
            var isLetterOrDigit = char.IsLetterOrDigit(c);
            var isSpace = c == ' ';
            var isPeriod = c == '.'; // add the period here to not replace it with the uderscore later
            var isProtectionChar = c == '>'; // to have fun with file protection
            var isGreaterThan127 = c > 127 && c < 256;
            var isShortSpecialChar = new List<char>(ShortNameSpecialChars).Contains(c);

            var result = isLetterOrDigit | isSpace | isPeriod | isProtectionChar | isGreaterThan127 | isShortSpecialChar;

            return result;
        }

        private static bool IsAllowedForLongName(char c)
        {
            var isShortNameAllowed = IsAllowedForShortName(c);
            var isLongSpecialChar = new List<char>(LongNameSpecialChars).Contains(c);

            var result = isShortNameAllowed | isLongSpecialChar;

            return result;
        }

        private static string StripIgnoredCharacters(string longName)
        {
            // 1. leading spaces
            var strippedName = longName.TrimStart(' ');

            // 2. trailing spaces and periods
            strippedName = strippedName.TrimEnd(' ', '.');

            return strippedName;
        }

        private static bool MatchNameInternal(CompositeDirectoryEntry compositeEntry, string name)
        {
            // compare the long and short names regardless the case
            var longNameMatch = string.Compare(compositeEntry.Name, name, StringComparison.OrdinalIgnoreCase) == 0;
            var shortNameMatch = string.Compare(compositeEntry.ShortName, name, StringComparison.OrdinalIgnoreCase) == 0;

            var result = longNameMatch | shortNameMatch;

            return result;
        }

        private static bool ContainsMatchingName(IDirectoryEntryContainer container, string name)
        {
            var result = false;

            // find a matching one in the container
            foreach (var directoryEntry in container.Entries)
            {
                var e = directoryEntry as CompositeDirectoryEntry;
                if (e == null)
                {
                    throw new InvalidOperationException($"The entry is not of type {typeof(CompositeDirectoryEntry)}");
                }

                if (MatchNameInternal(e, name))
                {
                    result = true;
                    break;
                }
            }

            return result;
        }

        private static void ValidateLongName(string longName, IDirectoryEntryContainer targetContainer)
        {
            // 1. empty
            // 2. too long
            // 3. contains illegal chars
            // 4. a duplicate

            if (longName.Length == 0)
            {
                throw new NameEmptyException($"The name becomes empty after stripping ignored characters: {longName}");
            }

            if (longName.Length > CompositeDirectoryEntry.MaxLongNameLength)
            {
                throw new NameTooLongException(
                    $"The name length ({longName.Length}) is larger than the maximum alowable value: {CompositeDirectoryEntry.MaxLongNameLength}");
            }

            foreach (var c in longName)
            {
                if (!IsAllowedForLongName(c))
                {
                    throw new NameHasIllegalCharsException($"The name has an illegal character: {c}");
                }
            }

            if (ContainsMatchingName(targetContainer, longName))
            {
                throw new NameAlreadyExistsException($"The name already exists: {longName}");
            }
        }

        private static void ValidateVolumeLabel(string volumeLabel)
        {
            // 1. empty
            // 2. too long
            // 3. contains illegal chars

            if (volumeLabel.Length == 0)
            {
                throw new NameEmptyException(
                    $"The volume label becomes empty after stripping ignored characters: {volumeLabel}");
            }

            if (volumeLabel.Length > VolumeLabelEntry.MaxLabelLength)
            {
                throw new NameTooLongException(
                    $"The volume label length ({volumeLabel.Length}) is larger than the maximum alowable value: {VolumeLabelEntry.MaxLabelLength}");
            }

            foreach (var c in volumeLabel)
            {
                if (!IsAllowedForShortName(c))
                {
                    throw new NameHasIllegalCharsException($"The volume label has an illegal character: {c}");
                }
            }
        }

        private static bool CollidesWithExistingShortName(string basisName, IDirectoryEntryContainer targetContainer)
        {
            var result = false;

            // does the container contain the same one (by the name)?
            foreach (var directoryEntry in targetContainer.Entries)
            {
                var e = directoryEntry as CompositeDirectoryEntry;
                if (e == null)
                {
                    throw new InvalidOperationException($"The entry is not of type {typeof(CompositeDirectoryEntry)}");
                }

                if (string.Compare(basisName, e.ShortName, StringComparison.OrdinalIgnoreCase) == 0)
                {
                    result = true;
                    break;
                }
            }

            return result;
        }

        private static string InsertNumericTail(
            string primaryPortion,
            string extensionPortion,
            IDirectoryEntryContainer targetContainer)
        {
            string shortName = null;

            string numericTail = null;

            // check the possible range for a free numeric tail
            for (var i = 1; i <= 999999; i++)
            {
                // the current tail
                numericTail = string.Concat("~", i, extensionPortion);

                // check the current tail with all entries
                foreach (var directoryEntry in targetContainer.Entries)
                {
                    var e = directoryEntry as CompositeDirectoryEntry;
                    if (e == null)
                    {
                        throw new InvalidOperationException(
                            $"The entry is not of type {typeof(CompositeDirectoryEntry)}");
                    }

                    if (e.ShortName.EndsWith(numericTail, StringComparison.OrdinalIgnoreCase))
                    {
                        // it is already used, reset it
                        numericTail = null;
                        break;
                    }
                }

                // is the current tail free?
                if (numericTail != null)
                {
                    // exclude the extension portion from the tail (for a convenient use later)
                    numericTail = string.Concat("~", i);
                    break;
                }
            }

            // did we find a free tail?
            if (numericTail != null)
            {
                // apply it

                var primaryPortionCopy = primaryPortion;

                // 1. primary portion max allowable length (due to the presence of the tail)
                var primaryPortionMaxLength = ShortDirectoryEntry.MaxNameMainPartLength - numericTail.Length;

                // 2. adjust the primary portion according to the previous step
                if (primaryPortionCopy.Length > primaryPortionMaxLength)
                {
                    primaryPortionCopy = primaryPortionCopy.Substring(0, primaryPortionMaxLength);
                }

                // 3. create a short name using the adjusted primary portion, the tail, and extension portion
                shortName = string.Concat(primaryPortionCopy, numericTail, extensionPortion);
            }

            return shortName;
        }

        private static string GenerateShortName(string longName, IDirectoryEntryContainer targetContainer)
        {
            string shortName;

            /////////////
            // Generally:
            // 1. Any spaces in the filenames should be ignored when converting to SFN.
            // 2. Ignore all dots except the last one. Leave out the other dots, just like the spaces.
            // 3. Commas and square brackets are changed to underscores.
            // 4. Case is not important, upper case and lower case characters are treated equally.
            // by Wiki (http://en.wikipedia.org/wiki/8.3_filename)
            /////////////

            // 1. strip spaces
            var tempResult = longName.Replace(" ", string.Empty);

            // 2. strip all periods except the last one
            var parts = tempResult.Split('.');

            // if there are more than one period in the long name
            if (parts.Length > 2)
            {
                // leave just the last period
                tempResult = string.Join(string.Empty, parts, 0, parts.Length - 1);
                tempResult = string.Concat(tempResult, ".", parts[parts.Length - 1]);
            }

            // 3. change chars that are not allowed in a short name to the underscore
            var convertionResult = string.Empty;

            // the flag will tell us if the convertion has illegal chars
            var lossyConvertion = false;

            foreach (var c in tempResult)
            {
                var toConcat = c;

                if (!IsAllowedForShortName(toConcat))
                {
                    toConcat = '_';
                    lossyConvertion = true;
                }

                convertionResult = string.Concat(convertionResult, toConcat);
            }

            // 4. convert to upper case
            tempResult = convertionResult.ToUpper();

            // 5. now we can copy chars into two portions of the short name: primary and extension
            var extensionPortion = string.Empty;

            // split the temp result into two possible parts (the period may not exist in the string at all)
            parts = tempResult.Split('.');

            // the primary portion should always exist
            var primaryPortion = parts[0];
            if (primaryPortion.Length > ShortDirectoryEntry.MaxNameMainPartLength)
            {
                // the length is limited!
                primaryPortion = primaryPortion.Substring(0, ShortDirectoryEntry.MaxNameMainPartLength);
            }

            // the extension portion exists only if there is a period in the string
            if (parts.Length == 2)
            {
                extensionPortion = parts[1];
                if (extensionPortion.Length > ShortDirectoryEntry.MaxNameExtensionLength)
                {
                    // the length is limited!
                    extensionPortion = extensionPortion.Substring(0, ShortDirectoryEntry.MaxNameExtensionLength);
                }

                // now insert a period to the beginning of the extension portion (for a convenient use later)
                extensionPortion = string.Concat(".", extensionPortion);
            }

            // 6. the last thing to do is to determine whether we need to use a numeric tail
            var basisName = string.Concat(primaryPortion, extensionPortion);

            if (!lossyConvertion &&
                string.Compare(longName, basisName, StringComparison.Ordinal) == 0 &&
                !CollidesWithExistingShortName(basisName, targetContainer))
            {
                shortName = basisName;
            }
            else
            {
                shortName = InsertNumericTail(primaryPortion, extensionPortion, targetContainer);
            }

            return shortName;
        }

        private static void ValidateEntry(IDirectoryEntry entry, string parameterName)
        {
            if (entry == null) throw new ArgumentNullException(nameof(entry));

            // support just this type
            if (!(entry is CompositeDirectoryEntry))
            {
                throw new ArgumentException(
                    $"The entry has an invalid type {entry.GetType()}",
                    parameterName);
            }
        }

        private static void ValidateContainer(IDirectoryEntryContainer container, string parameterName)
        {
            if (container == null) throw new ArgumentNullException(nameof(container));

            // all entries are of the known supported type
            foreach (var e in container.Entries)
            {
                if (!(e is CompositeDirectoryEntry))
                {
                    throw new ArgumentException(
                        $"The container contains an entry of an invalid type {e.GetType()}",
                        parameterName);
                }
            }
        }

        private static void ValidateEntryAgainstContainer(
            IDirectoryEntryContainer container,
            IDirectoryEntry entry,
            string parameterName)
        {
            var entryFound = false;

            // is the entry contained in the container?
            foreach (var e in container.Entries)
            {
                if (e == entry)
                {
                    entryFound = true;
                    break;
                }
            }

            if (!entryFound)
            {
                throw new ArgumentException("The entry is not contained in the container", parameterName);
            }
        }

        IDirectoryEntry IDirectoryEntryManager.CreateDotEntry()
        {
            // use the predefined parameters
            var dotShortEntry = new ShortDirectoryEntry
            {
                NameMainPart = DotEntryName,
                NameExtension = DotEntryExtension,
                EntryAttributes = DotEntryAttributes
            };

            // no long entries
            var dotCompositeEntry = new CompositeDirectoryEntry(
                dotShortEntry,
                new LongDirectoryEntry[0]);

            return dotCompositeEntry;
        }

        IDirectoryEntry IDirectoryEntryManager.CreateDotdotEntry()
        {
            // use the predefined parameters
            var dotdotShortEntry = new ShortDirectoryEntry
            {
                NameMainPart = DotdotEntryName,
                NameExtension = DotdotEntryExtension,
                EntryAttributes = DotdotEntryAttributes
            };

            // no long entries
            var dotdotCompositeEntry = new CompositeDirectoryEntry(
                dotdotShortEntry,
                new LongDirectoryEntry[0]);

            return dotdotCompositeEntry;
        }

        IDirectoryEntry IDirectoryEntryManager.CreateVolumeLabelEntry()
        {
            return new VolumeLabelEntry();
        }

        IDirectoryEntry IDirectoryEntryManager.CreateRootDirectoryEntry()
        {
            return new RootDirectoryEntry();
        }

        IDirectoryEntry IDirectoryEntryManager.CreateEntry(IDirectoryEntryContainer targetContainer, string name)
        {
            ValidateContainer(targetContainer, "targetContainer");

            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentException("The name must not be empty", nameof(name));
            }

            // remove all ignored chars and check whether it became empty
            var strippedName = StripIgnoredCharacters(name);
            ValidateLongName(strippedName, targetContainer);

            // try to generate a short name from the passed long one
            var shortName = GenerateShortName(strippedName, targetContainer);
            if (shortName == null)
            {
                throw new NameCollisionException($"The name causes a conflict that cannot be resolved: {strippedName}");
            }

            // create a composite entry, load it with the data, and rebuid it
            var compositeEntry = new CompositeDirectoryEntry();
            compositeEntry.LoadLongName(strippedName);
            compositeEntry.LoadShortName(shortName);
            compositeEntry.Rebuild();

            return compositeEntry;
        }

        void IDirectoryEntryManager.RenameEntry(
            IDirectoryEntry entry,
            IDirectoryEntryContainer targetContainer,
            string newName)
        {
            ValidateEntry(entry, "entry");
            ValidateContainer(targetContainer, "targetContainer");
            ValidateEntryAgainstContainer(targetContainer, entry, "entry");

            if (string.IsNullOrEmpty(newName))
            {
                throw new ArgumentException("The name must not be empty", nameof(newName));
            }

            // remove all ignored chars and check whether it became empty
            var strippedName = StripIgnoredCharacters(newName);
            ValidateLongName(strippedName, targetContainer);

            // try to generate a new short name from the passed long one
            var shortName = GenerateShortName(strippedName, targetContainer);
            if (shortName == null)
            {
                throw new NameCollisionException($"The name causes a conflict that cannot be resolved: {strippedName}");
            }

            // use the passed composite entry, load it with the data, and rebuid it
            var compositeEntry = (CompositeDirectoryEntry) entry;
            compositeEntry.LoadLongName(strippedName);
            compositeEntry.LoadShortName(shortName);
            compositeEntry.Rebuild();
        }

        void IDirectoryEntryManager.RenameVolumeLabelEntry(IDirectoryEntry volumeLabelEntry, string newName)
        {
            if (volumeLabelEntry == null) throw new ArgumentNullException(nameof(volumeLabelEntry));

            if (!(volumeLabelEntry is VolumeLabelEntry))
            {
                throw new ArgumentException(
                    "The specified entry is not a valid volume label entry",
                    nameof(volumeLabelEntry));
            }

            if (string.IsNullOrEmpty(newName))
            {
                throw new ArgumentException("The name must not be empty", nameof(newName));
            }

            // remove all ignored chars and check whether it became empty
            var strippedName = StripIgnoredCharacters(newName);
            ValidateVolumeLabel(strippedName);

            // volume labels as short entries use upper case chars
            var upperCasedLabel = strippedName.ToUpper();

            ((VolumeLabelEntry) volumeLabelEntry).Label = upperCasedLabel;
        }

        bool IDirectoryEntryManager.MatchEntryName(IDirectoryEntry entry, string name)
        {
            ValidateEntry(entry, "entry");

            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentException("The name must not be empty", nameof(name));
            }

            return MatchNameInternal(entry as CompositeDirectoryEntry, name);
        }

        IDirectoryEntry[] IDirectoryEntryManager.LoadEntries(Stream input, bool returnErroneousEntries)
        {
            if (input == null) throw new ArgumentNullException(nameof(input));

            if (!input.CanRead)
            {
                throw new ArgumentException("The input stream must be readable", nameof(input));
            }

            // this list we're going to populate with entries to return
            var entries = new List<IDirectoryEntry>();

            // a buffer to parse entries from
            var entryBytes = new byte[DirectoryEntryParser.DirectoryEntryLength];

            // this list will be accumulating long entries as we find them (to create a composire entry after)
            var longEntriesTempStorage = new List<LongDirectoryEntry>();

            // read bytes from the stream and parse them until
            // either the end of the stream has been reached or a "last" entry has been found
            DirectoryEntryParseResult result;
            do
            {
                // fill the buffer with the next portion of bytes
                var bytesRead = input.Read(entryBytes, 0, entryBytes.Length);

                // handle a possible end of the stream
                if (bytesRead == 0)
                {
                    break;
                }

                // do parse

                // a parsed entry
                object parsedEntry;
                result = DirectoryEntryParser.Parse(entryBytes, out parsedEntry);

                // did we find a long, short, or volume label entry?
                if (result == DirectoryEntryParseResult.LongEntry)
                {
                    // accumulate it for future use
                    longEntriesTempStorage.Add(parsedEntry as LongDirectoryEntry);
                }
                else if (result == DirectoryEntryParseResult.ShortEntry)
                {
                    // we have to create a composite entry now

                    // long entries are stored in reverse order
                    longEntriesTempStorage.Reverse();

                    // create an entry from the current short entry and a number of accumulated long entries
                    var entry = new CompositeDirectoryEntry(
                        parsedEntry as ShortDirectoryEntry,
                        longEntriesTempStorage);

                    // we don't need these anymore
                    longEntriesTempStorage.Clear();

                    // do we need to check for errors before returning the entry?
                    if (returnErroneousEntries)
                    {
                        // add as is
                        entries.Add(entry);
                    }
                    else
                    {
                        // check whether it is valid first
                        if (entry.IsValid())
                        {
                            entries.Add(entry);
                        }
                    }
                }
                else if (result == DirectoryEntryParseResult.VolumeLabelEntry)
                {
                    // we have found a volume label

                    // first, we have to check whether any long entries were found earlier.
                    // if they were, we remove them as orphans (since we didn't find a short entry for them)
                    longEntriesTempStorage.Clear();

                    // return the label
                    entries.Add(parsedEntry as VolumeLabelEntry);
                }
            } while (result != DirectoryEntryParseResult.LastEntry);

            return entries.ToArray();
        }

        int IDirectoryEntryManager.GetSize(IDirectoryEntry entry)
        {
            if (entry == null) throw new ArgumentNullException(nameof(entry));

            var size = 0;

            var directoryEntry = entry as CompositeDirectoryEntry;

            if (directoryEntry != null)
            {
                var compositeEntry = directoryEntry;

                size = compositeEntry.TotalSize;
            }
            else if (entry is VolumeLabelEntry)
            {
                size = DirectoryEntryParser.DirectoryEntryLength;
            }

            return size;
        }
    }
}