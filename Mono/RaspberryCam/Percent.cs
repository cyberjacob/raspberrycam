using System;

namespace RaspberryCam
{
    public struct Percent
    {
        private readonly int value;

        public Percent(int value)
        {
            this.value = value;
            if (value < 0 || value > 100)
                throw new ArgumentOutOfRangeException("value", "Percent must be between 0 and 100");
        }

        public int Value
        {
            get { return value; }
        }

        public static Percent From(int value)
        {
            return new Percent(value);
        }

        public static implicit operator Percent(int val)
        {
            return Percent.From(val);
        }

        public static implicit operator int(Percent percent)
        {
            return percent.Value;
        }

        public static Percent MinValue
        {
            get { return 0; }
        }

        public static Percent MaxValue
        {
            get { return 100; }
        }

        #region Equality Members

        public bool Equals(Percent other)
        {
            return value == other.value;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj is Percent && Equals((Percent) obj);
        }

        public override int GetHashCode()
        {
            return value;
        }

        #endregion

    }
}