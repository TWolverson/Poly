namespace Poly
{
    public enum LatticeVector
    {
        // lower plane x,y,-1
        LeftZeroDown = X.Left | Y.Zero | Z.Down,
        ZeroBackDown = X.Zero | Y.Back | Z.Down,
        RightZeroDown = X.Right | Y.Zero | Z.Down,
        ZeroForeDown = X.Zero | Y.Fore | Z.Down,
        // middle plane x,y,0
        LeftBackZero = X.Left | Y.Back | Z.Zero,
        LeftForeZero = X.Left | Y.Fore | Z.Zero,
        RightForeZero = X.Right | Y.Fore | Z.Zero,
        RightBackZero = X.Right | Y.Back | Z.Zero,
        // upper plane x,y,1
        LeftZeroUp = X.Left | Y.Zero | Z.Up,
        ZeroBackUp = X.Zero | Y.Back | Z.Up,
        RightZeroUp = X.Right | Y.Zero | Z.Up,
        ZeroForeUp = X.Zero | Y.Fore | Z.Up,
    }
}
