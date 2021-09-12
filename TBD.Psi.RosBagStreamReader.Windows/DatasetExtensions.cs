// Copyright (c) Carnegie Mellon University. All rights reserved.
// Licensed under the MIT license.

namespace TBD.Psi.RosBagStreamReader.Windows
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
    }
}
