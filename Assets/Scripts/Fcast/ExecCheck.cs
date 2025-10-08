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
    }
}
