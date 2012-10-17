using System;
using System.Collections.Generic;

namespace RaspberryCam
{
    public class PicturesCache
    {
        //Timespan bug with ARM version of MONO, so we will use int in milliseconds
        private readonly int duration;
        private readonly Dictionary<PicturesCacheKey, PicturesCacheValue> cacheValues;

        public PicturesCache(int duration)
        {
            this.duration = duration;
            cacheValues = new Dictionary<PicturesCacheKey, PicturesCacheValue>();
        }

        public bool IsExpired(PicturesCacheValue cacheValue)
        {
            Console.WriteLine("Checking expiration");

            var totalMilliseconds = DateTime.UtcNow.Subtract(cacheValue.Time).TotalMilliseconds;

            Console.WriteLine("totalMilliseconds {0}", totalMilliseconds);

            return totalMilliseconds >= duration;
        }


        public bool HasPicture(string devicePath, PictureSize pictureSize, Percent jpegCompressionRate)
        {
            var cacheKey = new PicturesCacheKey
                {
                    DevicePath = devicePath, PictureSize = pictureSize
                };
            if (!cacheValues.ContainsKey(cacheKey))
            {
                Console.WriteLine("ContainsKey {0}", false);
                return false;
            }

            var value = cacheValues[cacheKey];

            return !IsExpired(value);
        }

        public byte[] GetPicture(string devicePath, PictureSize pictureSize, Percent jpegCompressionRate)
        {
            var cacheKey = new PicturesCacheKey
                {
                    DevicePath = devicePath,
                    PictureSize = pictureSize
                };
            if (!cacheValues.ContainsKey(cacheKey))
                return new byte[0];

            Console.WriteLine("GetPicture from cache");

            return cacheValues[cacheKey].Data;
        }

        public void AddPicture(string devicePath, PictureSize pictureSize, Percent jpegCompressionRate, byte[] data)
        {
            var cacheKey = new PicturesCacheKey
                {
                    DevicePath = devicePath,
                    PictureSize = pictureSize
                };
            var cacheValue = new PicturesCacheValue
                {
                    Time = DateTime.UtcNow,
                    Data = data
                };
            if (!cacheValues.ContainsKey(cacheKey))
            {
                cacheValues.Add(cacheKey, cacheValue);
                return;
            }

            cacheValues[cacheKey] = cacheValue;
        }
    }
}