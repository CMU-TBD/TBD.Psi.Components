namespace TBD.Psi.RosBagStreamReader
{
    using System;
    using Microsoft.Psi.Data;
    public static class DatasetExtensions
    {
        public static Session AddSessionFromRosBagStore(
            this Dataset dataset,
            string storeName,
            string storePath,
            DateTime startTime,
            string sessionName = null,
            string partitionName = null)
        {
            return dataset.AddSessionFromStore(
                new RosBagStreamReader(
                    storeName,
                    storePath),
                sessionName,
                partitionName);
        }

        public static Partition<RosBagStreamReader> AddWaveFileStorePartition(
            this Session session,
            string storeName,
            string storePath,
            DateTime startTime,
            string partitionName = null)
        {
            return session.AddStorePartition(
                new RosBagStreamReader(
                    storeName,
                    storePath),
                partitionName);
        }
    }
}
