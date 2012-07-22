using System;
using System.Text;

namespace msUnit {
    public enum Result { Pass, Fail }

    struct TestDetails {
        public bool Passed;
        public string Name;
        public Exception Thrown;
        public TimeSpan Time;
        public string StdOut;
        public string StdErr;
    }
}
