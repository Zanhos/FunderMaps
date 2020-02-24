﻿using System;

namespace FunderMaps.ViewModels
{
    /// <summary>
    /// File download result model.
    /// </summary>
    public class FileDownloadOutputModel
    {
        /// <summary>
        /// Downloading url.
        /// </summary>
        public Uri Url { get; set; }

        /// <summary>
        /// Url validity in hours.
        /// </summary>
        public int UrlValid { get; set; }
    }
}