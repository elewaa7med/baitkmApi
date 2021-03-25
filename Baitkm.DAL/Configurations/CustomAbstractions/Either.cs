using System;

namespace Baitkm.DAL.Configurations.CustomAbstractions
{
    public abstract class Either < L, R>
    {
        public abstract T Match<T>(Func<L, T> f, Func<R, T> g);
        public abstract void Match(Action<L> f, Action<R> g);
        public abstract Either<T, R> Map<T>(Func<L, T> f);
        private Either()
        {

        }

        public static implicit operator Either<L, R>(L ell)
        {
            return new Left(ell);
        }

        public static implicit operator Either<L, R>(R arr)
        {
            return new Right(arr);
        }


        public sealed class Right : Either<L, R>
        {
            public readonly R Item;
            public Right(R item)
            {
                Item = item;
            }

            public override T Match<T>(Func<L, T> f, Func<R, T> g)
            {
                return g(Item);
            }

            public override void Match(Action<L> f, Action<R> g)
            {
                g(Item);
            }

            public override Either<T, R> Map<T>(Func<L, T> f)
            {
                return Item;
            }
        }

        public sealed class Left : Either<L, R>
        {
            public readonly L Item;
            public Left(L item)
            {
                Item = item;
            }

            public override T Match<T>(Func<L, T> f, Func<R, T> g)
            {
                return f(Item);
            }

            public override void Match(Action<L> f, Action<R> g)
            {
                f(Item);
            }

            public override Either<T, R> Map<T>(Func<L, T> f)
            {
                return f(Item);
            }
        }

    }
}
