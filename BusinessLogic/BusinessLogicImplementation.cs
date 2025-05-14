//____________________________________________________________________________________________________________________________________
//
//  Copyright (C) 2024, Mariusz Postol LODZ POLAND.
//
//  To be in touch join the community by pressing the `Watch` button and get started commenting using the discussion panel at
//
//  https://github.com/mpostol/TP/discussions/182
//
//_____________________________________________________________________________________________________________________________________

using System.Collections.Concurrent;
using System.Diagnostics;
using System.Drawing;
using System.Numerics;
using TP.ConcurrentProgramming.Data;
using UnderneathLayerAPI = TP.ConcurrentProgramming.Data.DataAbstractAPI;

namespace TP.ConcurrentProgramming.BusinessLogic
{
  internal class BusinessLogicImplementation : BusinessLogicAbstractAPI
  {
    #region ctor

    public BusinessLogicImplementation() : this(null)
    { }

    internal BusinessLogicImplementation(UnderneathLayerAPI? underneathLayer)
    {
      layerBellow = underneathLayer == null ? UnderneathLayerAPI.GetDataLayer() : underneathLayer;
      _collisionManager = new CollisionManager();
    }

    #endregion ctor

    #region BusinessLogicAbstractAPI

    public override void Dispose()
    {
      if (Disposed)
        throw new ObjectDisposedException(nameof(BusinessLogicImplementation));
      layerBellow.Dispose();
      _collisionManager.Dispose();
      Disposed = true;
    }

    public override void Start(int numberOfBalls, Action<IVector, IBall> upperLayerHandler)
    {
      if (Disposed)
        throw new ObjectDisposedException(nameof(BusinessLogicImplementation));
      if (upperLayerHandler == null)
        throw new ArgumentNullException(nameof(upperLayerHandler));

      layerBellow.Start(numberOfBalls, (pos, dataBall) =>
      {
        var logicBall = new Ball(dataBall);
        _collisionManager.RegisterBall(logicBall);
        _balls.TryAdd(dataBall, logicBall);
        upperLayerHandler(pos, logicBall);
      });
    }

    #endregion BusinessLogicAbstractAPI

    #region private

    private bool Disposed = false;

        private readonly UnderneathLayerAPI layerBellow;
    private CollisionManager _collisionManager;
    private readonly ConcurrentDictionary<Data.IBall, Ball> _balls = new();

    #endregion private

    #region SetCanvasSize
    public override void SetCanvasSize(double width, double height)
    {
      if (Disposed)
        throw new ObjectDisposedException(nameof(BusinessLogicImplementation));

      _collisionManager?.Dispose();

      layerBellow.SetCanvasSize(width, height);

      _collisionManager = new CollisionManager((int)width, (int)height);
      foreach (var ball in _balls.Values)
      {
        _collisionManager.RegisterBall(ball);
      }
    }
#endregion SetCanvasSize

    #region TestingInfrastructure

    [Conditional("DEBUG")]
    internal void CheckObjectDisposed(Action<bool> returnInstanceDisposed)
    {
      returnInstanceDisposed(Disposed);
    }

    #endregion TestingInfrastructure
  }

  internal class CollisionManager : IDisposable
  {
    private const int MaxBallsPerNode = 4;
    private readonly object _treeLock = new();
    private QuadTree _quadTree;
    private bool _isRunning = true;
    private Size _areaSize;
    private readonly Timer _collisionCheckTimer;

    public CollisionManager(int width = 3000, int height = 1500)
    {
      _areaSize = new Size(width, height);
      RebuildTree();
      _collisionCheckTimer = new Timer(_ => CheckCollisions(), null, 0, 16);
    }

    public void RegisterBall(Ball ball)
    {
      lock (_treeLock)
      {
        _quadTree.Insert(ball);
      }

      Task.Run(() => UpdateBall(ball));
    }

    private async void UpdateBall(Ball ball)
    {
      while (_isRunning)
      {
        lock (_treeLock)
        {
          _quadTree.Update(ball);
        }

        await Task.Delay(10);
      }
    }

    public void CheckCollisions()
    {
      lock (_treeLock)
      {
        var potentialCollisions = _quadTree.GetPotentialCollisions();
        foreach (var (a, b) in potentialCollisions)
        {
          if (CheckCollision(a, b))
          {
            ResolveCollision(a, b);
          }
        }
      }
    }

    private bool CheckCollision(Ball a, Ball b)
    {
      double dx = a.Position.x - b.Position.x;
      double dy = a.Position.y - b.Position.y;
      double distance = Math.Sqrt(dx * dx + dy * dy);
      return distance < (a.Diameter / 2 + b.Diameter / 2);
    }

    private void ResolveCollision(Ball a, Ball b)
    {
      Vector2 normal = new Vector2(
        (float)(b.Position.x - a.Position.x),
        (float)(b.Position.y - a.Position.y)
      );
      normal = Vector2.Normalize(normal);

      Vector2 relativeVelocity = new Vector2(
        (float)(b.Velocity.x - a.Velocity.x),
        (float)(b.Velocity.y - a.Velocity.y)
      );

      float impulse = Vector2.Dot(relativeVelocity, normal);
      if (impulse > 0) return; // Kolizja już się rozwiązuje

      float restitution = 0.8f; // Współczynnik restytucji (mniej niż 1 dla tłumienia)
      float j = -(1 + restitution) * impulse / (1 / (float)a.Mass + 1 / (float)b.Mass);

      a.SetVelocity(
        a.Velocity.x - (j * normal.X) / (float)a.Mass,
        a.Velocity.y - (j * normal.Y) / (float)a.Mass
      );

      b.SetVelocity(
        b.Velocity.x + (j * normal.X) / (float)b.Mass,
        b.Velocity.y + (j * normal.Y) / (float)b.Mass
      );
    }

    public void Dispose()
    {
      _collisionCheckTimer?.Dispose();
      _isRunning = false;
    }
    

    private void RebuildTree()
    {
      _quadTree = new QuadTree(
        new Rectangle(0, 0, _areaSize.Width, _areaSize.Height),
        MaxBallsPerNode
      );
    }
  }

  internal class QuadTree
  {
    private readonly Rectangle _bounds;
    private readonly int _maxBallsPerNode;
    private readonly List<Ball> _balls = new();
    private QuadTree[] _nodes;
    private readonly object _lock = new();

    public QuadTree(Rectangle bounds, int maxBallsPerNode)
    {
      _bounds = bounds;
      _maxBallsPerNode = maxBallsPerNode;
    }

    public void Insert(Ball ball)
    {
      lock (_lock)
      {
        if (_nodes != null)
        {
          int index = GetIndex(ball.Position);
          if (index != -1)
          {
            _nodes[index].Insert(ball);
            return;
          }
        }

        _balls.Add(ball);

        if (_balls.Count > _maxBallsPerNode && _nodes == null)
        {
          Subdivide();
          RedistributeBalls();
        }
      }
    }
    public void Update(Ball ball)
    {
      lock (_lock)
      {
        if (!_bounds.Contains(ball.Position))
        {
          Remove(ball);
          Insert(ball);
        }
      }
    }

    public IEnumerable<(Ball, Ball)> GetPotentialCollisions()
    {
      lock (_lock)
      {
        var collisions = new List<(Ball, Ball)>();

        for (int i = 0; i < _balls.Count; i++)
          for (int j = i + 1; j < _balls.Count; j++)
            collisions.Add((_balls[i], _balls[j]));

        if (_nodes != null)
          foreach (var node in _nodes)
            collisions.AddRange(node.GetPotentialCollisions());

        return collisions;
      }
    }

    private void Subdivide()
    {
      double halfWidth = _bounds.Width / 2;
      double halfHeight = _bounds.Height / 2;

      _nodes = new QuadTree[]
      {
            new QuadTree(new Rectangle(_bounds.X, _bounds.Y, halfWidth, halfHeight), _maxBallsPerNode),
            new QuadTree(new Rectangle(_bounds.X + halfWidth, _bounds.Y, halfWidth, halfHeight), _maxBallsPerNode),
            new QuadTree(new Rectangle(_bounds.X, _bounds.Y + halfHeight, halfWidth, halfHeight), _maxBallsPerNode),
            new QuadTree(new Rectangle(_bounds.X + halfWidth, _bounds.Y + halfHeight, halfWidth, halfHeight), _maxBallsPerNode)
      };
    }

    private void RedistributeBalls()
    {
      foreach (var ball in _balls.ToList())
      {
        int index = GetIndex(ball.Position);
        if (index != -1)
        {
          _nodes[index].Insert(ball);
          _balls.Remove(ball);
        }
      }
    }

    private int GetIndex(IVector position)
    {
      if (_nodes == null) return -1;

      bool top = position.y < _bounds.Y + _bounds.Height / 2;
      bool left = position.x < _bounds.X + _bounds.Width / 2;

      if (left)
        return top ? 0 : 2;
      else
        return top ? 1 : 3;
    }

    private void Remove(Ball ball)
    {
      _balls.Remove(ball);
      if (_nodes != null)
        foreach (var node in _nodes)
          node.Remove(ball);
    }
  }

  public struct Rectangle
  {
    public double X { get; }
    public double Y { get; }
    public double Width { get; }
    public double Height { get; }

    public Rectangle(double x, double y, double width, double height)
    {
      X = x;
      Y = y;
      Width = width;
      Height = height;
    }

    public bool Contains(IVector position)
    {
      return position.x >= X &&
             position.x <= X + Width &&
             position.y >= Y &&
             position.y <= Y + Height;
    }
  }
}