namespace ProjectCleaner;

internal sealed class PseudoProgressBar : UserControl
{
  public enum ProgressDockSide
  {
    Left,
    Right
  }

  public enum PseudoProgressState
  {
    progress, warn, error, complete
  }

  private readonly Panel _track;
  private readonly Panel _fill;
  private readonly Label _label;
  private readonly System.Windows.Forms.Timer _timer;
  private int _value;
  private int _total;
  private ProgressDockSide _dockSide;
  private PseudoProgressState _state;
  public override string Text
  {
    get => _label.Text;
    set
    {
      _label.Text = value;
      _label.BringToFront();
    }
  }

  public PseudoProgressState State
  {
    get => _state;
    set
    {
      _state = value;
      _fill.BackColor = value switch
      {
        PseudoProgressState.warn => Color.DarkKhaki,
        PseudoProgressState.error => Color.IndianRed,
        PseudoProgressState.complete => Color.ForestGreen,
        _ => Color.RoyalBlue,
      };
    }
  }


  public PseudoProgressBar()
  {
    Height = 22;
    _dockSide = ProgressDockSide.Left;
    _track = new Panel
    {
      Dock = DockStyle.Fill,
      BorderStyle = BorderStyle.Fixed3D,
      Margin = new Padding(0),
    };
    _fill = new Panel
    {
      Dock = DockStyle.Left,
      Width = 0,
      Height = 18,
      Margin = new Padding(1),
      BorderStyle = BorderStyle.None,
      BackColor = Color.RoyalBlue,
    };
    _label = new Label
    {
      Dock = DockStyle.Fill,
      TextAlign = ContentAlignment.MiddleCenter,
      ForeColor = Color.White,
      BackColor = Color.Transparent,
    };
    _timer = new System.Windows.Forms.Timer { Interval = 30 };
    _timer.Tick += (_, _) => UpdateWidth();
    _track.Controls.Add(_fill);
    _fill.Controls.Add(_label);
    _label.BringToFront();
    Controls.Add(_track);
    SetVisible(false);
    _state = PseudoProgressState.progress;
  }

  public ProgressDockSide DockSide
  {
    get => _dockSide;
    set
    {
      _dockSide = value;
      _fill.Dock = _dockSide == ProgressDockSide.Left ? DockStyle.Left : DockStyle.Right;
    }
  }

  public void Start(int current, int total)
  {
    SetVisible(true);
    _value = current;
    _total = total;
    UpdateWidth();
  }

  public void Update(int current, int total)
  {
    if (InvokeRequired)
    {
      _ = BeginInvoke(new Action<int, int>(Update), current, total);
      return;
    }
    _value = current;
    _total = total;
    UpdateWidth();
  }

  public void SetVisible(bool visible)
  {
    _track.Visible = visible;
    _fill.Visible = visible;
    _label.Visible = visible;
    if (visible)
    {
      _fill.Width = 0;
      _timer.Start();
    }
    else
    {
      _timer.Stop();
    }
  }

  public void HideProgress()
  {
    _track.Visible = false;
    _fill.Visible = false;
    _timer.Stop();
  }

  public void Reset()
  {
    _value = 0;
    _total = 0;
    _fill.Width = 0;
  }

  private void UpdateWidth()
  {
    if (!_track.Visible)
    {
      return;
    }

    if (_total <= 0)
    {
      // For pseudo-progress animation when total is unknown
      return;
    }

    int trackWidth = _track.Width;
    if (trackWidth <= 0)
    {
      return;
    }

    double ratio = (double)_value / _total;
    int maxWidth = trackWidth - 4;
    int targetWidth = (int)(maxWidth * ratio);
    int currentWidth = _fill.Width;

    if (targetWidth > currentWidth)
    {
      _fill.Width = targetWidth;
    }
    else if (targetWidth < currentWidth)
    {
      _fill.Width = targetWidth;
    }
  }
}