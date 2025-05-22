

namespace TP.ConcurrentProgramming.BusinessLogic
{
    public class Position : IPosition
    {
        #region IPosition
        public double x { get; init; }
        public double y { get; init; }
        #endregion IPosition

        public Position(double XComponent, double YComponent)
        {
            x = XComponent;
            y = YComponent;
        }
    }
} // do wyrzucenia

