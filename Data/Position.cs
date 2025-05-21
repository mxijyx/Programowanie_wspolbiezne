

namespace TP.ConcurrentProgramming.Data
{
    public class Position : IPosition
    {
        #region IPosition
        public double x { get; set; }
        public double y { get; set; }
        #endregion IPosition

        public Position(double XComponent, double YComponent)
        {
            x = XComponent;
            y = YComponent;
        }
    }
}
