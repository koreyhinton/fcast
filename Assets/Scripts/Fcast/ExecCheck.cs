namespace Fcast
{
    // Usage is similar to how my ned project structures imperative programs:
    //     if (check)
    //     {
    //         otherCheck.SomeConcreteKeyProperty = 'some_lookup_key';
    //         otherCheck.Exec();
    //     }
    //     if (otherCheck)
    //     {
    //        ;
    //     }
    public abstract class ExecCheck : IExec
    {
        protected bool Check { get; set; }
        public static implicit operator bool(ExecCheck o)
        {
            return o.Check;
        }
        public abstract void Exec();
        public static bool Nand(params bool[] checks)
        {
            foreach (var c in checks)
                if (!c)
                    return true;
            return false;
        }
        public static bool Nand(params ExecCheck[] checks)
        {
            foreach (var c in checks)
                if (!c)
                    return true;
            return false;
        }
        public static bool Nand(params object[] checks)
        {
            foreach (var c in checks)
                if (!((bool)c))
                    return true;
            return false;
        }
        public static bool Imply(bool a, bool b)
        {
            return !a || b;
        }
        public static bool Imply(ExecCheck a, ExecCheck b)
        {
            return !a || b;
        }
        public static bool Imply(object a, object b)
        {
            return !(bool)a || (bool)b;
        }
    }
}
