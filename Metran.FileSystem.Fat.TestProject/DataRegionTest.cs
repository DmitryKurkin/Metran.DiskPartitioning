using Microsoft.VisualStudio.TestTools.UnitTesting;

using System;
using System.IO;

using Metran.FileSystem.Fat.ClusterLayer;

namespace Metran.FileSystem.Fat.TestProject
{
    [TestClass]
    public class DataRegionTest
    {
        private TestContext testContextInstance;

        /// <summary>
        /// Gets or sets the test context which provides
        /// information about and functionality for the current test run.
        /// </summary>
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

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException), "The specified stream is a null reference")]
        public void ConstructorThrowsOnStreamNullReferenceTest()
        {
            Stream targetStream = null;
            long streamBasePosition = 0;
            int clustersCount = 0;
            int sectorsPerCluster = 0;

            DataRegion dataRegion = new DataRegion(targetStream, streamBasePosition, clustersCount, sectorsPerCluster);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException), "The specified stream in not readable")]
        public void ConstructorThrowsOnNonReadableStreamTest()
        {
            Stream targetStream = new MockStream(false, true, true);
            long streamBasePosition = 0;
            int clustersCount = 0;
            int sectorsPerCluster = 0;

            DataRegion dataRegion = new DataRegion(targetStream, streamBasePosition, clustersCount, sectorsPerCluster);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException), "The specified stream in not seekable")]
        public void ConstructorThrowsOnNonSeekableStreamTest()
        {
            Stream targetStream = new MockStream(true, false, true);
            long streamBasePosition = 0;
            int clustersCount = 0;
            int sectorsPerCluster = 0;

            DataRegion dataRegion = new DataRegion(targetStream, streamBasePosition, clustersCount, sectorsPerCluster);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException), "The specified stream in not writeable")]
        public void ConstructorThrowsOnNonWriteableStreamTest()
        {
            Stream targetStream = new MockStream(true, true, false);
            long streamBasePosition = 0;
            int clustersCount = 0;
            int sectorsPerCluster = 0;

            DataRegion dataRegion = new DataRegion(targetStream, streamBasePosition, clustersCount, sectorsPerCluster);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException), "The specified clusters count is zero")]
        public void ConstructorThrowsOnZeroClustersCountTest()
        {
            Stream targetStream = new MockStream(true, true, true);
            long streamBasePosition = 0;
            int clustersCount = 0;
            int sectorsPerCluster = 4;

            DataRegion dataRegion = new DataRegion(targetStream, streamBasePosition, clustersCount, sectorsPerCluster);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException), "The specified clusters count is negative")]
        public void ConstructorThrowsOnNegativeClustersCountTest()
        {
            Stream targetStream = new MockStream(true, true, true);
            long streamBasePosition = 0;
            int clustersCount = -100;
            int sectorsPerCluster = 4;

            DataRegion dataRegion = new DataRegion(targetStream, streamBasePosition, clustersCount, sectorsPerCluster);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException), "The specified sectors per cluster count is zero")]
        public void ConstructorThrowsOnZeroSectorsPerClusterTest()
        {
            Stream targetStream = new MockStream(true, true, true);
            long streamBasePosition = 0;
            int clustersCount = 100;
            int sectorsPerCluster = 0;

            DataRegion dataRegion = new DataRegion(targetStream, streamBasePosition, clustersCount, sectorsPerCluster);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException), "The specified sectors per cluster count is negative")]
        public void ConstructorThrowsOnNegativeSectorsPerClusterTest()
        {
            Stream targetStream = new MockStream(true, true, true);
            long streamBasePosition = 0;
            int clustersCount = 100;
            int sectorsPerCluster = -4;

            DataRegion dataRegion = new DataRegion(targetStream, streamBasePosition, clustersCount, sectorsPerCluster);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException), "The specified sectors per cluster count is too high")]
        public void ConstructorThrowsOnHugeSectorsPerClusterTest()
        {
            Stream targetStream = new MockStream(true, true, true);
            long streamBasePosition = 0;
            int clustersCount = 100;
            int sectorsPerCluster = int.MaxValue;

            DataRegion dataRegion = new DataRegion(targetStream, streamBasePosition, clustersCount, sectorsPerCluster);
        }

        #endregion

        [TestMethod]
        public void ClustersCountTest()
        {
            Stream targetStream = new MockStream(true, true, true);
            long streamBasePosition = 0;
            int clustersCount = 100;
            int sectorsPerCluster = 4;

            DataRegion dataRegion = new DataRegion(targetStream, streamBasePosition, clustersCount, sectorsPerCluster);

            int expected = clustersCount;
            int actual = (dataRegion as IDataRegion).ClustersCount;

            Assert.AreEqual<int>(
                expected,
                actual,
                "The clusters count is not correct");
        }

        [TestMethod]
        public void SectorsPerClusterTest()
        {
            Stream targetStream = new MockStream(true, true, true);
            long streamBasePosition = 0;
            int clustersCount = 100;
            int sectorsPerCluster = 4;

            DataRegion dataRegion = new DataRegion(targetStream, streamBasePosition, clustersCount, sectorsPerCluster);

            int expected = sectorsPerCluster;
            int actual = (dataRegion as IDataRegion).SectorsPerCluster;

            Assert.AreEqual<int>(
                expected,
                actual,
                "The sectors per cluster count is not correct");
        }

        #region ReadCluster tests

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException), "The specified cluster number is negative")]
        public void ReadClusterThrowsOnNegativeClusterNumberTest()
        {
            Stream targetStream = new MemoryStream();
            long streamBasePosition = 0;
            int clustersCount = 100;
            int sectorsPerCluster = 4;

            DataRegion dataRegion = new DataRegion(targetStream, streamBasePosition, clustersCount, sectorsPerCluster);

            (dataRegion as IDataRegion).ReadCluster(-10);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException), "The specified cluster number is too large")]
        public void ReadClusterThrowsOnLargeClusterNumberTest()
        {
            Stream targetStream = new MemoryStream();
            long streamBasePosition = 0;
            int clustersCount = 100;
            int sectorsPerCluster = 4;

            DataRegion dataRegion = new DataRegion(targetStream, streamBasePosition, clustersCount, sectorsPerCluster);

            (dataRegion as IDataRegion).ReadCluster(200);
        }

        [TestMethod]
        public void ReadClusterReturnsValidDataTest()
        {
            MemoryStream targetStream = new MemoryStream();
            int startSector = 50;
            int clustersCount = 100;
            int sectorsPerCluster = 2;

            DataRegion dataRegion = new DataRegion(targetStream, startSector, clustersCount, sectorsPerCluster);

            int oneClusterLength = sectorsPerCluster * DataRegion.BytesPerSector;

            // data for one cluster
            byte[] inputClusterData = new byte[oneClusterLength];
            for (int i = 0; i < inputClusterData.Length; i += 2)
            {
                inputClusterData[i] = 0x55;
                inputClusterData[i + 1] = 0xAA;
            }

            int cluster = 25;

            targetStream.Position = startSector * DataRegion.BytesPerSector + cluster * oneClusterLength;
            targetStream.Write(inputClusterData, 0, inputClusterData.Length);
            targetStream.Position = startSector * DataRegion.BytesPerSector;

            byte[] actualClusterData = (dataRegion as IDataRegion).ReadCluster(cluster);

            Assert.IsNotNull(actualClusterData, "The returned cluster data is a null reference");

            Assert.AreEqual<int>(
                oneClusterLength,
                actualClusterData.Length,
                "The returned cluster data length is invalid");

            for (int i = 0; i < actualClusterData.Length; i++)
            {
                Assert.AreEqual<int>(
                    inputClusterData[i],
                    actualClusterData[i],
                    "Byte number {0} was not returned correctly",
                    i);
            }
        }

        #endregion

        #region WriteCluster tests

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException), "The specified cluster number is negative")]
        public void WriteClusterThrowsOnNegativeClusterNumberTest()
        {
            Stream targetStream = new MemoryStream();
            long streamBasePosition = 0;
            int clustersCount = 100;
            int sectorsPerCluster = 4;

            DataRegion dataRegion = new DataRegion(targetStream, streamBasePosition, clustersCount, sectorsPerCluster);

            byte[] clusterData = new byte[sectorsPerCluster * DataRegion.BytesPerSector];
            (dataRegion as IDataRegion).WriteCluster(-10, clusterData);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException), "The specified cluster number is too large")]
        public void WriteClusterThrowsOnLargeClusterNumberTest()
        {
            Stream targetStream = new MemoryStream();
            long streamBasePosition = 0;
            int clustersCount = 100;
            int sectorsPerCluster = 4;

            DataRegion dataRegion = new DataRegion(targetStream, streamBasePosition, clustersCount, sectorsPerCluster);

            byte[] clusterData = new byte[sectorsPerCluster * DataRegion.BytesPerSector];
            (dataRegion as IDataRegion).WriteCluster(200, clusterData);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException), "The specified cluster data is a null reference")]
        public void WriteClusterThrowsOnClusterDataNullReferenceTest()
        {
            Stream targetStream = new MemoryStream();
            long streamBasePosition = 0;
            int clustersCount = 100;
            int sectorsPerCluster = 4;

            DataRegion dataRegion = new DataRegion(targetStream, streamBasePosition, clustersCount, sectorsPerCluster);

            byte[] clusterData = null;
            (dataRegion as IDataRegion).WriteCluster(0, clusterData);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException), "The specified cluster data length is invalid")]
        public void WriteClusterThrowsOnInvalidDataLengthTest()
        {
            Stream targetStream = new MemoryStream();
            long streamBasePosition = 0;
            int clustersCount = 100;
            int sectorsPerCluster = 4;

            DataRegion dataRegion = new DataRegion(targetStream, streamBasePosition, clustersCount, sectorsPerCluster);

            byte[] clusterData = new byte[2 * sectorsPerCluster * DataRegion.BytesPerSector + 1];
            (dataRegion as IDataRegion).WriteCluster(0, clusterData);
        }

        [TestMethod]
        public void WriteClusterWritesValidDataTest()
        {
            MemoryStream targetStream = new MemoryStream();
            int startSector = 50;
            int clustersCount = 100;
            int sectorsPerCluster = 2;

            DataRegion dataRegion = new DataRegion(targetStream, startSector, clustersCount, sectorsPerCluster);

            int oneClusterLength = sectorsPerCluster * DataRegion.BytesPerSector;

            // data for one cluster
            byte[] inputClusterData = new byte[oneClusterLength];
            for (int i = 0; i < inputClusterData.Length; i += 2)
            {
                inputClusterData[i] = 0x55;
                inputClusterData[i + 1] = 0xAA;
            }

            int cluster = 25;

            (dataRegion as IDataRegion).WriteCluster(cluster, inputClusterData);

            byte[] actualClusterData = new byte[oneClusterLength];

            targetStream.Position = startSector * DataRegion.BytesPerSector + cluster * oneClusterLength;
            targetStream.Read(actualClusterData, 0, actualClusterData.Length);

            for (int i = 0; i < actualClusterData.Length; i++)
            {
                Assert.AreEqual<int>(
                    inputClusterData[i],
                    actualClusterData[i],
                    "Byte number {0} was not written correctly",
                    i);
            }
        }

        #endregion
    }
}