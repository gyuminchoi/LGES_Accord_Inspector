﻿namespace Service.Database.Models
{
    public class RecordData
    {
        public string DateTime { get; set; }
        public string ParcelBarcode { get; set; }
        public string ProductBarcode { get; set; }
        public string ImagePath { get; set; }

        public RecordData(string dateTime, string parcelBarcode, string productBarcode, string imagePath)
        {
            DateTime = dateTime;
            ParcelBarcode = parcelBarcode;
            ProductBarcode = productBarcode;
            ImagePath = imagePath;
        }
        public RecordData()
        {
            
        }
    }
}
