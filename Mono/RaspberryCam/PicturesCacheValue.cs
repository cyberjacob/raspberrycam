using System;

namespace RaspberryCam
{
    public struct PicturesCacheValue
    {
        public byte[] Data;
        public DateTime Time;

        #region Equality Members

        public bool Equals(PicturesCacheValue other)
        {
            return Equals(Data, other.Data) && Time.Equals(other.Time);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj is PicturesCacheValue && Equals((PicturesCacheValue) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((Data != null ? Data.GetHashCode() : 0)*397) ^ Time.GetHashCode();
            }
        }

        #endregion
    }
}