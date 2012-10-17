namespace RaspberryCam
{
    public struct PicturesCacheKey
    {
        public PictureSize PictureSize;
        public string DevicePath;

        #region Equality Members

        public bool Equals(PicturesCacheKey other)
        {
            return PictureSize.Equals(other.PictureSize) && string.Equals(DevicePath, other.DevicePath);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj is PicturesCacheKey && Equals((PicturesCacheKey) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (PictureSize.GetHashCode()*397) ^ (DevicePath != null ? DevicePath.GetHashCode() : 0);
            }
        }

        #endregion
    }
}