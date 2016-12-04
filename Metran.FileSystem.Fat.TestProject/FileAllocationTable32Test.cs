using Microsoft.VisualStudio.TestTools.UnitTesting;

using System;
using System.IO;

using Metran.FileSystem.Fat.ClusterLayer;

namespace Metran.FileSystem.Fat.TestProject
{
    [TestClass]
    public class FileAllocationTable32Test
    {
        private TestContext testContextInstance;

        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        #region Constructor tests

        /// <summary>
        /// A negative clusters count is specified
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentException), "The data region clusters is negative")]
        public void ConstructorThrowsOnNegativeClustersCountTest()
        {
            int dataRegionClustersCount = -11;
            byte mediaDescriptor = 0xF8;
            FileSystemInformation fileSystemInfo = new FileSystemInformation();

            FileAllocationTableFat32 target = new FileAllocationTableFat32(dataRegionClustersCount, mediaDescriptor, fileSystemInfo);
        }

        /// <summary>
        /// A huge clusters count is specified
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentException), "The data region clusters is too high")]
        public void ConstructorThrowsOnHugeClustersCountTest()
        {
            int dataRegionClustersCount = FileAllocationTableFat32_Accessor.BadClusterMark + 11;
            byte mediaDescriptor = 0xF8;
            FileSystemInformation fileSystemInfo = new FileSystemInformation();

            FileAllocationTableFat32 target = new FileAllocationTableFat32(dataRegionClustersCount, mediaDescriptor, fileSystemInfo);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException), "The specified file system info is a null reference")]
        public void CtorThrowsOnFileSystemInfoNullReferenceTest()
        {
            FileAllocationTableFat32 target = new FileAllocationTableFat32(100, 0xF8, null);
        }

        /// <summary>
        /// The table length and 2 reserved clusters are initialized correctly
        /// </summary>
        [TestMethod]
        public void ConstructorInitializesTableCorrectlyTest()
        {
            int dataRegionClustersCount = 11;
            byte mediaDescriptor = 0xF8;
            FileSystemInformation fileSystemInfo = new FileSystemInformation();

            FileAllocationTableFat32 target = new FileAllocationTableFat32(dataRegionClustersCount, mediaDescriptor, fileSystemInfo);

            Assert.IsNotNull(
                target.RawTable,
                "The table has not been initialized");

            int expected = dataRegionClustersCount + 2;
            int actual = target.RawTable.Length;
            Assert.AreEqual<int>(
                expected,
                actual,
                "The table length is invalid");

            expected = 0x0FFFFF00 | mediaDescriptor;
            actual = target.RawTable[0];
            Assert.AreEqual<int>(
                expected,
                actual,
                "The first cluster is not initialized correctly");

            expected = FileAllocationTableFat32_Accessor.EocMark;
            actual = target.RawTable[1];
            Assert.AreEqual<int>(
                expected,
                actual,
                "The second cluster is not initialized correctly");

            expected = 0;
            for (int i = 2; i < target.RawTable.Length; i++)
            {
                actual = target.RawTable[i];
                Assert.AreEqual<int>(
                    expected,
                    actual,
                    "The cluster {0} is not initialized correctly",
                    i);
            }
        }

        /// <summary>
        /// The free clusters count gets set correctly after an initialization
        /// </summary>
        [TestMethod]
        public void ConstructorUpdatesFileSystemInfoTest()
        {
            int dataRegionClustersCount = 11;
            byte mediaDescriptor = 0xF8;
            FileSystemInformation fileSystemInfo = new FileSystemInformation();

            FileAllocationTableFat32 target = new FileAllocationTableFat32(dataRegionClustersCount, mediaDescriptor, fileSystemInfo);

            int expected = dataRegionClustersCount;
            int actual = (fileSystemInfo as IFileSystemInformation).FreeClusters;

            Assert.AreEqual<int>(
                expected,
                actual,
                "The free clusters count is not initialized correctly");
        }

        #endregion

        #region AllocateFirstCluster tests

        /// <summary>
        /// The first free cluster is used for an allocation
        /// </summary>
        [TestMethod]
        public void AllocateFirstClusterUsesFirstFreeClusterTest()
        {
            int dataRegionClustersCount = 5;
            byte mediaDescriptor = 0xF8;
            FileSystemInformation fileSystemInfo = new FileSystemInformation();

            FileAllocationTableFat32 target = new FileAllocationTableFat32(dataRegionClustersCount, mediaDescriptor, fileSystemInfo);

            target.RawTable[2] = FileAllocationTableFat32_Accessor.BadClusterMark;
            target.RawTable[3] = FileAllocationTableFat32_Accessor.BadClusterMark;
            target.RawTable[4] = FileAllocationTableFat32_Accessor.BadClusterMark;

            int expected = 5;
            int actual = (target as IFileAllocationTable).AllocateFirstCluster();

            Assert.AreEqual<int>(
                expected,
                actual,
                "The first free cluster was not used for the allocation");
        }

        /// <summary>
        /// The first free cluster is found correctly
        /// </summary>
        [TestMethod]
        public void AllocateFirstClusterFindFirstFreeClusterCorrectlyTest()
        {
            int dataRegionClustersCount = 5;
            byte mediaDescriptor = 0xF8;
            FileSystemInformation fileSystemInfo = new FileSystemInformation();

            FileAllocationTableFat32 target = new FileAllocationTableFat32(dataRegionClustersCount, mediaDescriptor, fileSystemInfo);

            // points to the last cluster in the table
            target.RawTable[6] = FileAllocationTableFat32_Accessor.BadClusterMark;

            int expected = 2;
            int actual = 0;

            try
            {
                actual = (target as IFileAllocationTable).AllocateFirstCluster();

                Assert.AreEqual<int>(
                    expected,
                    actual,
                    "The first free cluster was not found correctly");
            }
            catch (InvalidOperationException e)
            {
                Assert.Fail("The first free cluster was not found correctly. An exception was caught: {0}", e);
            }
        }

        /// <summary>
        /// The allocated cluster is initialized with the correct value
        /// </summary>
        [TestMethod]
        public void AllocateFirstClusterUsesCorrectInitialValueTest()
        {
            int dataRegionClustersCount = 5;
            byte mediaDescriptor = 0xF8;
            FileSystemInformation fileSystemInfo = new FileSystemInformation();

            FileAllocationTableFat32 target = new FileAllocationTableFat32(dataRegionClustersCount, mediaDescriptor, fileSystemInfo);

            int firstFreeCluster = (target as IFileAllocationTable).AllocateFirstCluster();

            int expected = FileAllocationTableFat32_Accessor.EocMark;
            int actual = target.RawTable[firstFreeCluster];

            Assert.AreEqual<int>(
                expected,
                actual,
                "The allocated cluster was not initialized correctly");
        }

        /// <summary>
        /// An exception upon an allocation on a full table
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(FileAllocationTableFullException), "The table is full")]
        public void AllocateFirstClusterThrowsOnFullTableTest()
        {
            int dataRegionClustersCount = 5;
            byte mediaDescriptor = 0xF8;
            FileSystemInformation fileSystemInfo = new FileSystemInformation();

            FileAllocationTableFat32 target = new FileAllocationTableFat32(dataRegionClustersCount, mediaDescriptor, fileSystemInfo);

            for (int i = 2; i < target.RawTable.Length; i++)
            {
                target.RawTable[i] = FileAllocationTableFat32_Accessor.BadClusterMark;
            }

            (target as IFileAllocationTable).AllocateFirstCluster();
        }

        /// <summary>
        /// The file system info gets updated correctly
        /// </summary>
        [TestMethod]
        public void AllocateFirstClusterUpdatesFileSystemInfoTest()
        {
            int dataRegionClustersCount = 5;
            byte mediaDescriptor = 0xF8;
            FileSystemInformation fileSystemInfo = new FileSystemInformation();

            FileAllocationTableFat32 target = new FileAllocationTableFat32(dataRegionClustersCount, mediaDescriptor, fileSystemInfo);

            int firstCluster = (target as IFileAllocationTable).AllocateFirstCluster();

            int expected = dataRegionClustersCount - 1;
            int actual = (fileSystemInfo as IFileSystemInformation).FreeClusters;
            Assert.AreEqual<int>(
                expected,
                actual,
                "The free clusters count was not updated correctly");

            expected = firstCluster;
            actual = (fileSystemInfo as IFileSystemInformation).LastAllocatedCluster;
            Assert.AreEqual<int>(
                expected,
                actual,
                "The last allocated cluster was not updated correctly");
        }

        /// <summary>
        /// The reserved 4 high-order bits of a cluster are preserved upon an allocation
        /// </summary>
        [TestMethod]
        public void AllocateFirstClusterPreservesReservedClusterHighOrderBitsTest()
        {
            int dataRegionClustersCount = 5;
            byte mediaDescriptor = 0xF8;
            FileSystemInformation fileSystemInfo = new FileSystemInformation();

            FileAllocationTableFat32 target = new FileAllocationTableFat32(dataRegionClustersCount, mediaDescriptor, fileSystemInfo);

            target.RawTable[2] = unchecked((int)0xF0000000);

            (target as IFileAllocationTable).AllocateFirstCluster();

            int expected = unchecked((int)0xF0000000 | FileAllocationTableFat32_Accessor.EocMark);
            int actual = target.RawTable[2];

            Assert.AreEqual<int>(
                expected,
                actual,
                "The reserved high-order bits were not preserved");
        }

        /// <summary>
        /// The reserved 4 high-order bits of a cluster are preserved upon an allocation
        /// </summary>
        [TestMethod]
        public void AllocateFirstClusterIgnoresReservedClusterHighOrderBitsUponAllocationTest()
        {
            int dataRegionClustersCount = 5;
            byte mediaDescriptor = 0xF8;
            FileSystemInformation fileSystemInfo = new FileSystemInformation();

            FileAllocationTableFat32 target = new FileAllocationTableFat32(dataRegionClustersCount, mediaDescriptor, fileSystemInfo);

            target.RawTable[2] = unchecked((int)0xF0000000);

            int expected = 2;
            int actual = (target as IFileAllocationTable).AllocateFirstCluster();

            Assert.AreEqual<int>(
                expected,
                actual,
                "The reserved high-order bits were not ignored upon the allocation");
        }

        #endregion

        #region AllocateNextCluster tests

        /// <summary>
        /// The next free cluster is used for an allocation
        /// </summary>
        [TestMethod]
        public void AllocateNextClusterUsesNextFreeClusterTest()
        {
            int dataRegionClustersCount = 5;
            byte mediaDescriptor = 0xF8;
            FileSystemInformation fileSystemInfo = new FileSystemInformation();

            FileAllocationTableFat32 target = new FileAllocationTableFat32(dataRegionClustersCount, mediaDescriptor, fileSystemInfo);

            target.RawTable[2] = FileAllocationTableFat32_Accessor.BadClusterMark;
            target.RawTable[3] = FileAllocationTableFat32_Accessor.EocMark;

            int previousCluster = 3;

            target.RawTable[previousCluster] = FileAllocationTableFat32_Accessor.EocMark;
            target.RawTable[previousCluster + 1] = FileAllocationTableFat32_Accessor.BadClusterMark;

            int expected = previousCluster + 2;
            int actual = (target as IFileAllocationTable).AllocateNextCluster(previousCluster);

            Assert.AreEqual<int>(
                expected,
                actual,
                "The next free cluster was not used for the allocation");
        }

        /// <summary>
        /// The next free cluster is found correctly
        /// </summary>
        [TestMethod]
        public void AllocateNextClusterFindsNextFreeClusterCorrectlyTest()
        {
            int dataRegionClustersCount = 5;
            byte mediaDescriptor = 0xF8;
            FileSystemInformation fileSystemInfo = new FileSystemInformation();

            FileAllocationTableFat32 target = new FileAllocationTableFat32(dataRegionClustersCount, mediaDescriptor, fileSystemInfo);

            int previousCluster = 5; // points to the pre-last cluster in the table

            target.RawTable[previousCluster] = FileAllocationTableFat32_Accessor.EocMark;
            target.RawTable[previousCluster + 1] = FileAllocationTableFat32_Accessor.BadClusterMark;

            int expected = 2;
            int actual = 0;

            try
            {
                actual = (target as IFileAllocationTable).AllocateNextCluster(previousCluster);

                Assert.AreEqual<int>(
                    expected,
                    actual,
                    "The next free cluster was not found correctly");
            }
            catch (InvalidOperationException e)
            {
                Assert.Fail("The next free cluster was not found correctly. An exception was caught: {0}", e);
            }
        }

        /// <summary>
        /// The allocated cluster is initialized with the correct value. The two clusters are connected into a chain
        /// </summary>
        [TestMethod]
        public void AllocateNextClusterBuildsCorrectClusterChainTest()
        {
            int dataRegionClustersCount = 5;
            byte mediaDescriptor = 0xF8;
            FileSystemInformation fileSystemInfo = new FileSystemInformation();

            FileAllocationTableFat32 target = new FileAllocationTableFat32(dataRegionClustersCount, mediaDescriptor, fileSystemInfo);

            int previousCluster = 3;

            target.RawTable[previousCluster] = FileAllocationTableFat32_Accessor.EocMark;
            target.RawTable[previousCluster + 1] = FileAllocationTableFat32_Accessor.BadClusterMark;

            int nextFreeCluster = (target as IFileAllocationTable).AllocateNextCluster(previousCluster);

            int expected = FileAllocationTableFat32_Accessor.EocMark;
            int actual = target.RawTable[nextFreeCluster];
            Assert.AreEqual<int>(
                expected,
                actual,
                "The allocated cluster was not initialized correctly");

            expected = nextFreeCluster;
            actual = target.RawTable[previousCluster];
            Assert.AreEqual<int>(
                expected,
                actual,
                "The cluster chain was not built correctly (the previous cluster is not pointing to the second one)");
        }

        /// <summary>
        /// A small cluster number is specified
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException), "The cluster number points to the reserved area")]
        public void AllocateNextClusterThrowsOnSmallClusterNumberTest()
        {
            int dataRegionClustersCount = 5;
            byte mediaDescriptor = 0xF8;
            FileSystemInformation fileSystemInfo = new FileSystemInformation();

            FileAllocationTableFat32 target = new FileAllocationTableFat32(dataRegionClustersCount, mediaDescriptor, fileSystemInfo);

            (target as IFileAllocationTable).AllocateNextCluster(0);
        }

        /// <summary>
        /// A large cluster number is specified
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException), "The cluster number points outside the table")]
        public void AllocateNextClusterThrowsOnLargeClusterNumberTest()
        {
            int dataRegionClustersCount = 5;
            byte mediaDescriptor = 0xF8;
            FileSystemInformation fileSystemInfo = new FileSystemInformation();

            FileAllocationTableFat32 target = new FileAllocationTableFat32(dataRegionClustersCount, mediaDescriptor, fileSystemInfo);

            (target as IFileAllocationTable).AllocateNextCluster(8);
        }

        /// <summary>
        /// An exception upon an allocation on a full table
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(FileAllocationTableFullException), "The table is full")]
        public void AllocateNextClusterThrowsOnFullTableTest()
        {
            int dataRegionClustersCount = 5;
            byte mediaDescriptor = 0xF8;
            FileSystemInformation fileSystemInfo = new FileSystemInformation();

            FileAllocationTableFat32 target = new FileAllocationTableFat32(dataRegionClustersCount, mediaDescriptor, fileSystemInfo);

            for (int i = 2; i < target.RawTable.Length; i++)
            {
                target.RawTable[i] = FileAllocationTableFat32_Accessor.EocMark;
            }

            (target as IFileAllocationTable).AllocateNextCluster(3);
        }

        /// <summary>
        /// The file system info gets updated correctly
        /// </summary>
        [TestMethod]
        public void AllocateNextClusterUpdatesFileSystemInfoTest()
        {
            int dataRegionClustersCount = 5;
            byte mediaDescriptor = 0xF8;
            FileSystemInformation fileSystemInfo = new FileSystemInformation();

            FileAllocationTableFat32 target = new FileAllocationTableFat32(dataRegionClustersCount, mediaDescriptor, fileSystemInfo);

            int firstCluster = (target as IFileAllocationTable).AllocateFirstCluster();
            int secondCluster = (target as IFileAllocationTable).AllocateNextCluster(firstCluster);

            int expected = dataRegionClustersCount - 2;
            int actual = (fileSystemInfo as IFileSystemInformation).FreeClusters;
            Assert.AreEqual<int>(
                expected,
                actual,
                "The free clusters count was not updated correctly");

            expected = secondCluster;
            actual = (fileSystemInfo as IFileSystemInformation).LastAllocatedCluster;
            Assert.AreEqual<int>(
                expected,
                actual,
                "The last allocated cluster was not updated correctly");
        }

        /// <summary>
        /// The reserved 4 high-order bits of a cluster are preserved upon an allocation
        /// </summary>
        [TestMethod]
        public void AllocateNextClusterPreservesReservedClusterHighOrderBitsTest()
        {
            int dataRegionClustersCount = 5;
            byte mediaDescriptor = 0xF8;
            FileSystemInformation fileSystemInfo = new FileSystemInformation();

            FileAllocationTableFat32 target = new FileAllocationTableFat32(dataRegionClustersCount, mediaDescriptor, fileSystemInfo);

            int firstCluster = (target as IFileAllocationTable).AllocateFirstCluster();

            target.RawTable[2] = unchecked((int)0xF0000000);

            (target as IFileAllocationTable).AllocateNextCluster(firstCluster);

            int expected = unchecked((int)0xF0000000 | 0x2);
            int actual = target.RawTable[2];

            Assert.AreEqual<int>(
                expected,
                actual,
                "The reserved high-order bits were not preserved");
        }

        #endregion

        #region DeallocateClusterChain tests

        /// <summary>
        /// A chain of clusters gets freed correctly
        /// </summary>
        [TestMethod]
        public void DeallocateClusterChainTest()
        {
            int dataRegionClustersCount = 5;
            byte mediaDescriptor = 0xF8;
            FileSystemInformation fileSystemInfo = new FileSystemInformation();

            FileAllocationTableFat32 target = new FileAllocationTableFat32(dataRegionClustersCount, mediaDescriptor, fileSystemInfo);

            target.RawTable[2] = 3;
            target.RawTable[3] = 5;
            target.RawTable[5] = FileAllocationTableFat32_Accessor.EocMark;

            int firstCluster = 2;
            (target as IFileAllocationTable).DeallocateClusterChain(firstCluster);

            int expected = 0;
            int actual = target.RawTable[2];
            Assert.AreEqual<int>(
                expected,
                actual,
                "The first cluster in the chain was not freed");

            expected = 0;
            actual = target.RawTable[3];
            Assert.AreEqual<int>(
                expected,
                actual,
                "The second cluster in the chain was not freed");

            expected = 0;
            actual = target.RawTable[5];
            Assert.AreEqual<int>(
                expected,
                actual,
                "The third cluster in the chain was not freed");
        }

        /// <summary>
        /// A small cluster number is specified
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException), "The cluster number points to the reserved area")]
        public void DeallocateClusterChainThrowsOnSmallClusterNumberTest()
        {
            int dataRegionClustersCount = 5;
            byte mediaDescriptor = 0xF8;
            FileSystemInformation fileSystemInfo = new FileSystemInformation();

            FileAllocationTableFat32 target = new FileAllocationTableFat32(dataRegionClustersCount, mediaDescriptor, fileSystemInfo);

            (target as IFileAllocationTable).DeallocateClusterChain(0);
        }

        /// <summary>
        /// A large cluster number is specified
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException), "The cluster number points outside the table")]
        public void DeallocateClusterChainThrowsOnLargeClusterNumberTest()
        {
            int dataRegionClustersCount = 5;
            byte mediaDescriptor = 0xF8;
            FileSystemInformation fileSystemInfo = new FileSystemInformation();

            FileAllocationTableFat32 target = new FileAllocationTableFat32(dataRegionClustersCount, mediaDescriptor, fileSystemInfo);

            (target as IFileAllocationTable).DeallocateClusterChain(8);
        }

        /// <summary>
        /// The file system info gets updated correctly
        /// </summary>
        [TestMethod]
        public void DeallocateClusterChainUpdatesFileSystemInfoTest()
        {
            int dataRegionClustersCount = 5;
            byte mediaDescriptor = 0xF8;
            FileSystemInformation fileSystemInfo = new FileSystemInformation();

            FileAllocationTableFat32 target = new FileAllocationTableFat32(dataRegionClustersCount, mediaDescriptor, fileSystemInfo);

            int firstCluster = (target as IFileAllocationTable).AllocateFirstCluster();
            int secondCluster = (target as IFileAllocationTable).AllocateNextCluster(firstCluster);

            (target as IFileAllocationTable).DeallocateClusterChain(firstCluster);

            int expected = dataRegionClustersCount;
            int actual = (fileSystemInfo as IFileSystemInformation).FreeClusters;
            Assert.AreEqual<int>(
                expected,
                actual,
                "The free clusters count was not updated correctly");
        }

        /// <summary>
        /// The reserved 4 high-order bits of a cluster are preserved upon an allocation
        /// </summary>
        [TestMethod]
        public void DeallocateClusterChainPreservesReservedClusterHighOrderBitsTest()
        {
            int dataRegionClustersCount = 5;
            byte mediaDescriptor = 0xF8;
            FileSystemInformation fileSystemInfo = new FileSystemInformation();

            FileAllocationTableFat32 target = new FileAllocationTableFat32(dataRegionClustersCount, mediaDescriptor, fileSystemInfo);

            int firstCluster = (target as IFileAllocationTable).AllocateFirstCluster();
            int secondCluster = (target as IFileAllocationTable).AllocateNextCluster(firstCluster);

            target.RawTable[2] |= unchecked((int)0xF0000000);

            (target as IFileAllocationTable).DeallocateClusterChain(firstCluster);

            int expected = unchecked((int)0xF0000000);
            int actual = target.RawTable[2];

            Assert.AreEqual<int>(
                expected,
                actual,
                "The reserved high-order bits were not preserved");
        }

        #endregion

        #region MarkBad tests

        /// <summary>
        /// A cluster is marked as a "bad" one correctly
        /// </summary>
        [TestMethod]
        public void MarkBadTest()
        {
            int dataRegionClustersCount = 5;
            byte mediaDescriptor = 0xF8;
            FileSystemInformation fileSystemInfo = new FileSystemInformation();

            FileAllocationTableFat32 target = new FileAllocationTableFat32(dataRegionClustersCount, mediaDescriptor, fileSystemInfo);

            int cluster = 3;
            (target as IFileAllocationTable).MarkBad(cluster);

            int expected = FileAllocationTableFat32_Accessor.BadClusterMark;
            int actual = target.RawTable[cluster];

            Assert.AreEqual<int>(
                expected,
                actual,
                "The cluster was not marked as a \"bad\" one correctly");
        }

        /// <summary>
        /// A small cluster number is specified
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException), "The cluster number points to the reserved area")]
        public void MarkBadThrowsOnSmallClusterNumberTest()
        {
            int dataRegionClustersCount = 5;
            byte mediaDescriptor = 0xF8;
            FileSystemInformation fileSystemInfo = new FileSystemInformation();

            FileAllocationTableFat32 target = new FileAllocationTableFat32(dataRegionClustersCount, mediaDescriptor, fileSystemInfo);

            (target as IFileAllocationTable).MarkBad(0);
        }

        /// <summary>
        /// A large cluster number is specified
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException), "The cluster number points outside the table")]
        public void MarkBadThrowsOnLargeClusterNumberTest()
        {
            int dataRegionClustersCount = 5;
            byte mediaDescriptor = 0xF8;
            FileSystemInformation fileSystemInfo = new FileSystemInformation();

            FileAllocationTableFat32 target = new FileAllocationTableFat32(dataRegionClustersCount, mediaDescriptor, fileSystemInfo);

            (target as IFileAllocationTable).MarkBad(8);
        }

        /// <summary>
        /// The file system info gets updated correctly
        /// </summary>
        [TestMethod]
        public void MarkBadUpdatesFileSystemInfoTest()
        {
            int dataRegionClustersCount = 5;
            byte mediaDescriptor = 0xF8;
            FileSystemInformation fileSystemInfo = new FileSystemInformation();

            FileAllocationTableFat32 target = new FileAllocationTableFat32(dataRegionClustersCount, mediaDescriptor, fileSystemInfo);

            (target as IFileAllocationTable).MarkBad(2);

            int expected = dataRegionClustersCount - 1;
            int actual = (fileSystemInfo as IFileSystemInformation).FreeClusters;
            Assert.AreEqual<int>(
                expected,
                actual,
                "The free clusters count was not updated correctly");
        }

        #endregion

        #region TraverseCluster tests

        /// <summary>
        /// The next cluster in a chain gets returned correctly
        /// </summary>
        [TestMethod]
        public void TraverseClusterTest()
        {
            int dataRegionClustersCount = 5;
            byte mediaDescriptor = 0xF8;
            FileSystemInformation fileSystemInfo = new FileSystemInformation();

            FileAllocationTableFat32 target = new FileAllocationTableFat32(dataRegionClustersCount, mediaDescriptor, fileSystemInfo);

            target.RawTable[3] = 5;

            int currentCluster = 3;

            int expected = 5;
            int actual = (target as IFileAllocationTable).TraverseCluster(currentCluster);

            Assert.AreEqual<int>(
                expected,
                actual,
                "The next cluster in the chain was not returned correctly");
        }

        /// <summary>
        /// A small cluster number is specified
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException), "The cluster number points to the reserved area")]
        public void TraverseClusterThrowsOnSmallClusterNumberTest()
        {
            int dataRegionClustersCount = 5;
            byte mediaDescriptor = 0xF8;
            FileSystemInformation fileSystemInfo = new FileSystemInformation();

            FileAllocationTableFat32 target = new FileAllocationTableFat32(dataRegionClustersCount, mediaDescriptor, fileSystemInfo);

            (target as IFileAllocationTable).TraverseCluster(0);
        }

        /// <summary>
        /// A large cluster number is specified
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException), "The cluster number points outside the table")]
        public void TraverseClusterThrowsOnLargeClusterNumberTest()
        {
            int dataRegionClustersCount = 5;
            byte mediaDescriptor = 0xF8;
            FileSystemInformation fileSystemInfo = new FileSystemInformation();

            FileAllocationTableFat32 target = new FileAllocationTableFat32(dataRegionClustersCount, mediaDescriptor, fileSystemInfo);

            (target as IFileAllocationTable).TraverseCluster(8);
        }

        #endregion

        #region IsLastCluster tests

        [TestMethod]
        public void IsLastClusterTest()
        {
            int dataRegionClustersCount = 5;
            byte mediaDescriptor = 0xF8;
            FileSystemInformation fileSystemInfo = new FileSystemInformation();

            FileAllocationTableFat32 target = new FileAllocationTableFat32(dataRegionClustersCount, mediaDescriptor, fileSystemInfo);

            target.RawTable[3] = FileAllocationTableFat32_Accessor.EocMark;

            int currentCluster = 3;

            bool expected = true;
            bool actual = (target as IFileAllocationTable).IsLastCluster(currentCluster);

            Assert.AreEqual<bool>(
                expected,
                actual,
                "The next cluster in the chain was not returned correctly");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException), "The cluster number points to the reserved area")]
        public void IsLastClusterThrowsOnSmallClusterNumberTest()
        {
            int dataRegionClustersCount = 5;
            byte mediaDescriptor = 0xF8;
            FileSystemInformation fileSystemInfo = new FileSystemInformation();

            FileAllocationTableFat32 target = new FileAllocationTableFat32(dataRegionClustersCount, mediaDescriptor, fileSystemInfo);

            (target as IFileAllocationTable).IsLastCluster(0);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException), "The cluster number points outside the table")]
        public void IsLastClusterThrowsOnLargeClusterNumberTest()
        {
            int dataRegionClustersCount = 5;
            byte mediaDescriptor = 0xF8;
            FileSystemInformation fileSystemInfo = new FileSystemInformation();

            FileAllocationTableFat32 target = new FileAllocationTableFat32(dataRegionClustersCount, mediaDescriptor, fileSystemInfo);

            (target as IFileAllocationTable).IsLastCluster(8);
        }

        #endregion

        #region TruncateClusterChain tests

        /// <summary>
        /// A chain of clusters gets truncated correctly
        /// </summary>
        [TestMethod]
        public void TruncateClusterChainTest()
        {
            int dataRegionClustersCount = 5;
            byte mediaDescriptor = 0xF8;
            FileSystemInformation fileSystemInfo = new FileSystemInformation();

            FileAllocationTableFat32 target = new FileAllocationTableFat32(dataRegionClustersCount, mediaDescriptor, fileSystemInfo);

            target.RawTable[2] = 3;
            target.RawTable[3] = 4;
            target.RawTable[4] = 5;
            target.RawTable[5] = FileAllocationTableFat32_Accessor.EocMark;

            int lastClusterInUse = 3;

            (target as IFileAllocationTable).TruncateClusterChain(lastClusterInUse);

            int expected = 0;
            int actual = target.RawTable[5];
            Assert.AreEqual<int>(
                expected,
                actual,
                "The fourth cluster was not freed");

            expected = 0;
            actual = target.RawTable[4];
            Assert.AreEqual<int>(
                expected,
                actual,
                "The third cluster was not freed");

            expected = FileAllocationTableFat32_Accessor.EocMark;
            actual = target.RawTable[3];
            Assert.AreEqual<int>(
                expected,
                actual,
                "The second cluster was not marked as last cluster");
        }

        /// <summary>
        /// A small cluster number is specified
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException), "The cluster number points to the reserved area")]
        public void TruncateClusterChainThrowsOnSmallClusterNumberTest()
        {
            int dataRegionClustersCount = 5;
            byte mediaDescriptor = 0xF8;
            FileSystemInformation fileSystemInfo = new FileSystemInformation();

            FileAllocationTableFat32 target = new FileAllocationTableFat32(dataRegionClustersCount, mediaDescriptor, fileSystemInfo);

            (target as IFileAllocationTable).TruncateClusterChain(0);
        }

        /// <summary>
        /// A large cluster number is specified
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException), "The cluster number points outside the table")]
        public void TruncateClusterChainThrowsOnLargeClusterNumberTest()
        {
            int dataRegionClustersCount = 5;
            byte mediaDescriptor = 0xF8;
            FileSystemInformation fileSystemInfo = new FileSystemInformation();

            FileAllocationTableFat32 target = new FileAllocationTableFat32(dataRegionClustersCount, mediaDescriptor, fileSystemInfo);

            (target as IFileAllocationTable).TruncateClusterChain(8);
        }

        /// <summary>
        /// The file system info gets updated correctly
        /// </summary>
        [TestMethod]
        public void TruncateClusterChainUpdatesFileSystemInfoTest()
        {
            int dataRegionClustersCount = 5;
            byte mediaDescriptor = 0xF8;
            FileSystemInformation fileSystemInfo = new FileSystemInformation();

            FileAllocationTableFat32 target = new FileAllocationTableFat32(dataRegionClustersCount, mediaDescriptor, fileSystemInfo);

            int firstCluster = (target as IFileAllocationTable).AllocateFirstCluster();
            int secondCluster = (target as IFileAllocationTable).AllocateNextCluster(firstCluster);
            int thirdCluster = (target as IFileAllocationTable).AllocateNextCluster(secondCluster);
            int fourthCluster = (target as IFileAllocationTable).AllocateNextCluster(thirdCluster);

            (target as IFileAllocationTable).TruncateClusterChain(secondCluster);

            int expected = dataRegionClustersCount - 2;
            int actual = (fileSystemInfo as IFileSystemInformation).FreeClusters;
            Assert.AreEqual<int>(
                expected,
                actual,
                "The free clusters count was not updated correctly");
        }

        /// <summary>
        /// The reserved 4 high-order bits of a cluster are preserved upon an allocation
        /// </summary>
        [TestMethod]
        public void TruncateClusterChainPreservesReservedClusterHighOrderBitsTest()
        {
            int dataRegionClustersCount = 5;
            byte mediaDescriptor = 0xF8;
            FileSystemInformation fileSystemInfo = new FileSystemInformation();

            FileAllocationTableFat32 target = new FileAllocationTableFat32(dataRegionClustersCount, mediaDescriptor, fileSystemInfo);

            int firstCluster = (target as IFileAllocationTable).AllocateFirstCluster();
            int secondCluster = (target as IFileAllocationTable).AllocateNextCluster(firstCluster);
            int thirdCluster = (target as IFileAllocationTable).AllocateNextCluster(secondCluster);
            int fourthCluster = (target as IFileAllocationTable).AllocateNextCluster(thirdCluster);

            target.RawTable[4] |= unchecked((int)0xF0000000);

            (target as IFileAllocationTable).TruncateClusterChain(secondCluster);

            int expected = unchecked((int)0xF0000000);
            int actual = target.RawTable[4];

            Assert.AreEqual<int>(
                expected,
                actual,
                "The reserved high-order bits were not preserved");
        }

        [TestMethod]
        public void TruncateClusterChainHandlesLastClusterCorrectlyTest()
        {
            int dataRegionClustersCount = 5;
            byte mediaDescriptor = 0xF8;
            FileSystemInformation fileSystemInfo = new FileSystemInformation();

            FileAllocationTableFat32 target = new FileAllocationTableFat32(dataRegionClustersCount, mediaDescriptor, fileSystemInfo);

            target.RawTable[2] = 3;
            target.RawTable[3] = 4;
            target.RawTable[4] = 5;
            target.RawTable[5] = FileAllocationTableFat32_Accessor.EocMark;

            (target as IFileAllocationTable).TruncateClusterChain(5);

            Assert.AreEqual<int>(
                3,
                target.RawTable[2],
                "The first cluster was not preserved");
            Assert.AreEqual<int>(
                4,
                target.RawTable[3],
                "The second cluster was not preserved");
            Assert.AreEqual<int>(
                5,
                target.RawTable[4],
                "The third cluster was not preserved");
            Assert.AreEqual<int>(
                FileAllocationTableFat32_Accessor.EocMark,
                target.RawTable[5],
                "The fourth cluster was not preserved");
        }

        #endregion

        /// <summary>
        /// The free clusters count gets returned correctly
        /// </summary>
        [TestMethod]
        public void FreeClustersTest()
        {
            int dataRegionClustersCount = 5;
            byte mediaDescriptor = 0xF8;
            FileSystemInformation fileSystemInfo = new FileSystemInformation();

            FileAllocationTableFat32 target = new FileAllocationTableFat32(dataRegionClustersCount, mediaDescriptor, fileSystemInfo);

            int expected = dataRegionClustersCount;
            int actual = (target as IFileAllocationTable).FreeClusters;
            Assert.AreEqual<int>(
                expected,
                actual,
                "The free clusters count is not correct after the initialization");
        }

        /// <summary>
        /// The last used cluster gets returned correctly
        /// </summary>
        [TestMethod]
        public void LastUsedClusterTest()
        {
            int dataRegionClustersCount = 5;
            byte mediaDescriptor = 0xF8;
            FileSystemInformation fileSystemInfo = new FileSystemInformation();

            FileAllocationTableFat32 target = new FileAllocationTableFat32(dataRegionClustersCount, mediaDescriptor, fileSystemInfo);

            int lastUsedCluster = 3;

            target.RawTable[lastUsedCluster] = FileAllocationTableFat32_Accessor.EocMark;

            int expected = lastUsedCluster;
            int actual = (target as IFileAllocationTable).LastUsedCluster;

            Assert.AreEqual(
                expected,
                actual,
                "The last used cluster was not returned correctly");
        }

        /// <summary>
        /// The table is flushed into a stream correctly
        /// </summary>
        [TestMethod]
        public void FlushTest()
        {
            int dataRegionClustersCount = 5;
            byte mediaDescriptor = 0xF8;
            FileSystemInformation fileSystemInfo = new FileSystemInformation();

            FileAllocationTableFat32 target = new FileAllocationTableFat32(dataRegionClustersCount, mediaDescriptor, fileSystemInfo);

            int firstCluster = (target as IFileAllocationTable).AllocateFirstCluster();
            int secondCluster = (target as IFileAllocationTable).AllocateNextCluster(firstCluster);
            int thirdCluster = (target as IFileAllocationTable).AllocateNextCluster(secondCluster);
            int fourthCluster = (target as IFileAllocationTable).AllocateNextCluster(thirdCluster);

            MemoryStream output = new MemoryStream();
            (target as IFileAllocationTable).Save(output);

            byte[] buffer = output.GetBuffer();

            int expected;
            int actual;
            for (int i = 0; i < target.RawTable.Length; i++)
            {
                expected = target.RawTable[i];
                actual = buffer[i * 4 + 3] << 24 | buffer[i * 4 + 2] << 16 | buffer[i * 4 + 1] << 8 | buffer[i * 4];

                Assert.AreEqual<int>(
                    expected,
                    actual,
                    "The table element {0} was not flushed correctly",
                    i);
            }
        }
    }
}