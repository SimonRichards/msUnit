using System;

namespace msUnit {
    internal enum Result { Pass, Fail }

    struct TestDetails {
        public bool Passed;
        public string Name;
        public Exception Thrown;
        public TimeSpan Time;
    }
}
