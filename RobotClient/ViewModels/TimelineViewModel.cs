using System.Windows.Media.Animation;
using RobotInterface.Timeline;
using System.Windows.Controls;
using System.Windows.Shapes;
using System.Windows.Input;
using System.Diagnostics;
using Caliburn.Micro;
using System.Windows;
using System;
using RobotInterface.Helpers;
using System.Windows.Documents;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace RobotInterface.ViewModels
{
    public class TimelineViewModel : Screen, IHandle<Clock>
    {
        public TimelineViewModel(
            IEventAggregator eventAggregator)
        {
            // Event Aggregator
            _eventAggregator = eventAggregator;
            _eventAggregator.Subscribe(this);

            DialogEventAggregatorProvider.EA.Subscribe(this);

            // Timeline initialisatiors
            FloaterPos = 0;
            TimeDuration();
        }

        #region Private Members

        // EventAggregator
        private IEventAggregator _eventAggregator { get; }

        #endregion

        #region Timeline

        #region Timeline Properties

        private BindableCollection<TimeLine> _Timelines = new BindableCollection<TimeLine>();
        private TimeSpan totalTime = new TimeSpan(0, 0, 10);
        private TimeSpan _MouseMovedInSeconds;
        private string _CurrentTimeStr = "0";
        private bool _MouseStatus = false;
        private double _MouseDistanceMoved;
        private Duration _StoryDuration;
        private double _MouseCurrentPosX;
        private bool _MouseClickedEvent;
        private string _DebugString;
        private int _FloaterPos = 0;
        private int _MousePosX = 0;
        private Clock _Clock;

        public bool LeftButtonDown { get; set; }
        public int TimeLineIdx { get; set; }
        public int TimeLineEventIdx { get; set; }
        public TimeSpan mouseDownStartTime { get; set; }
        public double mouseDownX { get; set; }

        /// <summary>
        /// FLoater position
        /// </summary>
        public int FloaterPos
        {
            get { return _FloaterPos; }
            set
            {
                _FloaterPos = value;
                GetCurrentTimeStr();
                NotifyOfPropertyChange(() => FloaterPos);
            }
        }

        /// <summary>
        /// Mouse click status
        /// </summary>
        public bool MouseStatus
        {
            get { return _MouseStatus; }
            set => Set(ref _MouseStatus, value);
        }

        /// <summary>
        /// Position of the mouse within canvas (actually goes out of bound to the right but dont know why)
        /// </summary>
        public int MousePosX
        {
            get { return _MousePosX; }
            set => Set(ref _MousePosX, value);
        }

        /// <summary>
        /// Current time string
        /// </summary>
        public string CurrentTimeStr
        {
            get { return _CurrentTimeStr; }
            set => Set(ref _CurrentTimeStr, value);
        }

        /// <summary>
        /// Duration of a story 
        /// </summary>
        public Duration StoryDuration
        {
            get { return _StoryDuration; }
            set => Set(ref _StoryDuration, value);
        }

        /// <summary>
        /// Clock object
        /// </summary>
        public Clock Clock
        {
            get { return _Clock; }
            set { _Clock = value; }
        }

        private string _CurrentTime;

        /// <summary>
        /// Current time 
        /// </summary>
        public string CurrentTime
        {
            get { return _CurrentTime; }
            set
            {
                _CurrentTime = value;
                NotifyOfPropertyChange(() => CurrentTime);
            }
        }

        /// <summary>
        /// Debug string initialisation
        /// </summary>
        public string DebugString
        {
            get { return _DebugString; }
            set => Set(ref _DebugString, value);
        }

        private double _TimeLineEventPosX;

        /// <summary>
        /// Timeline event pos x
        /// </summary>
        public double TimeLineEventPosX
        {
            get { return _TimeLineEventPosX; }
            set => Set(ref _TimeLineEventPosX, value);
        }

        private TimeSpan _TimelineStart;

        /// <summary>
        /// Timeline start 
        /// </summary>
        public TimeSpan TimelineStart
        {
            get { return _TimelineStart; }
            set => Set(ref _TimelineStart, value);
        }

        private TimeSpan _TimelineDuration;

        /// <summary>
        /// Timeline duration
        /// </summary>
        public TimeSpan TimelineDuration
        {
            get { return _TimelineDuration; }
            set { _TimelineDuration = value; }
        }

        /// <summary>
        /// List of timelines
        /// </summary>
        public BindableCollection<TimeLine> Timelines
        {
            get { return _Timelines; }
            set { _Timelines = value; }
        }

        private TimeLineEvent _SelectedTimeLineEvent;

        /// <summary>
        /// Selected timeline event
        /// </summary>
        public TimeLineEvent SelectedTimeLineEvent
        {
            get { return _SelectedTimeLineEvent; }
            set => Set(ref _SelectedTimeLineEvent, value);
        }

        /// <summary>
        /// Mouse moved in seconds
        /// </summary>
        public TimeSpan MouseMovedInSeconds
        {
            get { return _MouseMovedInSeconds; }
            set => Set(ref _MouseMovedInSeconds, value);
        }

        /// <summary>
        /// Mouse distance moved in px
        /// </summary>
        public double MouseDistanceMoved
        {
            get { return _MouseDistanceMoved; }
            set => Set(ref _MouseDistanceMoved, value);
        }

        /// <summary>
        /// Mouse clicked
        /// </summary>
        public bool MouseClickedEvent
        {
            get { return _MouseClickedEvent; }
            set => Set(ref _MouseClickedEvent, value);
        }

        /// <summary>
        /// Current mouse position
        /// </summary>
        public double MouseCurrentPosX
        {
            get { return _MouseCurrentPosX; }
            set => Set(ref _MouseCurrentPosX, value);
        }

        #endregion

        #region Timeline Methods

        /// <summary>
        /// Mouse Move Event within canvas
        /// </summary>
        /// <param name="source"></param>
        public void MouseMove(Canvas source)
        {
            Point p = Mouse.GetPosition(source);
            MousePosX = Convert.ToInt32(p.X);

            if (MouseStatus)
            {
                if (MousePosX > 650)
                    MousePosX = 650;

                FloaterPos = MousePosX;
                TimeDuration();
            }
        }

        /// <summary>
        /// Gets seconds out of position of the Floater
        /// </summary>
        /// <returns></returns>
        private void TimeDuration()
        {
            double totTime = totalTime.TotalMilliseconds;
            if (FloaterPos != 0)
            {
                totTime -= (totTime * FloaterPos / 650);
            }

            StoryDuration = new Duration(TimeSpan.FromMilliseconds(totTime));
        }

        /// <summary>
        /// Get current time str
        /// </summary>
        private void GetCurrentTimeStr()
        {
            int totTime = (int)totalTime.TotalMilliseconds;
            TimeSpan curTime = new TimeSpan();
            if (FloaterPos != 0)
            {
                curTime = new TimeSpan(0, 0, 0, 0, Convert.ToInt32(totTime * FloaterPos / 500));
                CurrentTimeStr = curTime.ToString();
            }
            else if (FloaterPos == 0)
            {
                curTime = new TimeSpan(0, 0, 0, 0, 0);
                CurrentTimeStr = curTime.ToString();
            }
        }

        /// <summary>
        /// Stop button click event
        /// </summary>
        public void StopButton()
        {
            FloaterPos = 0;
            TimeDuration();
        }

        /// <summary>
        /// Start button click event
        /// </summary>
        public void StartButton()
        {
        }

        /// <summary>
        /// Mouse Down event inside canvas
        /// </summary>
        public void MouseDown()
        {
            MouseStatus = true;
        }

        /// <summary>
        /// Mouse release
        /// </summary>
        public void MouseUp()
        {
            MouseStatus = false;
        }

        /// <summary>
        /// Method for feeding the timeline with dummy data
        /// </summary>
        public void FeedTimelines()
        {
            Timelines.Clear();
            TimeLine first = new TimeLine() { Duration = new TimeSpan(0, 0, 20) };
            first.Events.Add(new TimeLineEvent() { Start = new TimeSpan(0, 0, 1), Duration = new TimeSpan(0, 0, 2), Name = "Vskok1" });
            //first.Events.Add(new TimeLineEvent() { Start = new TimeSpan(0, 0, 4), Duration = new TimeSpan(0, 0, 5), Name = "Vskok2" });
            //first.Events.Add(new TimeLineEvent() { Start = new TimeSpan(0, 0, 13), Duration = new TimeSpan(0, 0, 3), Name = "Vskok3" });
            Timelines.Add(first);

            TimeLine second = new TimeLine() { Duration = new TimeSpan(0, 0, 25) };
            second.Events.Add(new TimeLineEvent() { Start = new TimeSpan(0, 0, 2), Duration = new TimeSpan(0, 0, 3), Name = "Visje1" });
            second.Events.Add(new TimeLineEvent() { Start = new TimeSpan(0, 0, 7), Duration = new TimeSpan(0, 0, 1), Name = "Visje2" });
            second.Events.Add(new TimeLineEvent() { Start = new TimeSpan(0, 0, 0, 9, 5), Duration = new TimeSpan(0, 0, 0, 4, 5), Name = "Visje3" });
            second.Events.Add(new TimeLineEvent() { Start = new TimeSpan(0, 0, 19), Duration = new TimeSpan(0, 0, 3), Name = "Visje4" });
            Timelines.Add(second);

            TimeLine third = new TimeLine() { Duration = new TimeSpan(0, 0, 20) };
            third.Events.Add(new TimeLineEvent() { Start = new TimeSpan(0, 0, 2), Duration = new TimeSpan(0, 0, 3), Name = "Buksy1" });
            third.Events.Add(new TimeLineEvent() { Start = new TimeSpan(0, 0, 7), Duration = new TimeSpan(0, 0, 1), Name = "Buksy2" });
            third.Events.Add(new TimeLineEvent() { Start = new TimeSpan(0, 0, 0, 9, 5), Duration = new TimeSpan(0, 0, 0, 4, 5), Name = "Buksy2" });
            third.Events.Add(new TimeLineEvent() { Start = new TimeSpan(0, 0, 16), Duration = new TimeSpan(0, 0, 3), Name = "Buksy2" });
            Timelines.Add(third);
        }

        /// <summary>
        /// Left button down on timeline event (rectangle)
        /// </summary>
        /// <param name="rect"></param>
        /// <param name="args"></param>
        public void TimeLineEventLeftDown(object rect, MouseButtonEventArgs args)
        {
            args.Handled = false;

            if (!(rect is Rectangle selectedRect))
                return;

            LeftButtonDown = true;
            MouseClickedEvent = LeftButtonDown;

            Grid rectParent = selectedRect.Parent as Grid;
            TimeLineEvent clickedEvent = selectedRect.DataContext as TimeLineEvent;

            
            if (MouseHitType == HitType.None) return;

            LastPoint = Mouse.GetPosition(rectParent);
            DragInProgress = true;

            // Finding the selected timeline event 
            foreach (var timeline in Timelines)
            {
                foreach (var tlEvent in timeline.Events)
                {
                    if (tlEvent == clickedEvent)
                    {
                        SelectedTimeLineEvent = tlEvent;
                        TimeLineIdx = Timelines.IndexOf(timeline);
                        TimeLineEventIdx = timeline.Events.IndexOf(tlEvent);
                    }
                }
            }

            DebugString = SelectedTimeLineEvent.Name;

            mouseDownX = Mouse.GetPosition(rectParent).X;
            TimeLineEventPosX = mouseDownX;

            mouseDownStartTime = SelectedTimeLineEvent.Start;

            Timelines.Refresh();
            selectedRect.CaptureMouse();

            args.Handled = true;
        }

        /// <summary>
        /// Left button up on timeline event (rectangle)
        /// </summary>
        /// <param name="rect"></param>
        public void TimeLineEventLeftUp(object rect)
        {
            LeftButtonDown = false;
            DragInProgress = false;

            Mouse.OverrideCursor = Cursors.Arrow;

            if (!(rect is Rectangle selectedRect))
                return;

            selectedRect.ReleaseMouseCapture();
            MouseClickedEvent = LeftButtonDown;
        }

        /// <summary>
        /// Mouse move in timeline
        /// </summary>
        /// <param name="rect"></param>
        /// <param name="args"></param>
        public void TimeLineEventMouseMove(object rect, MouseEventArgs args)
        {
            args.Handled = false;
            if (!(rect is Thumb selectedRect))
                return;

            Grid rectParent = selectedRect.Parent as Grid;

            MouseHitType = SetHitType(selectedRect, rectParent);

            SetMouseCursor();

            if (!LeftButtonDown)
                return;

            if (MouseHitType == HitType.Left || MouseHitType == HitType.Right)
            {
                if (DragInProgress)
                {
                    // See how much the mouse has moved.
                    Point point = Mouse.GetPosition(rectParent);
                    double offset_x = point.X - LastPoint.X;
                    Debug.WriteLine($"Current point: {point.X}");
                    Debug.WriteLine($"Last point: {LastPoint.X}");
                    Debug.WriteLine($"Offset: {offset_x}");
                    Point p = selectedRect.TranslatePoint(new Point(0, 0), rectParent);


                    // Get the rectangle's current position.
                    //double new_x = Canvas.GetLeft(selectedRect);
                    double new_x = p.X;
                    double thisX = Mouse.GetPosition(rectParent).X;
                    double new_width = selectedRect.Width;

                    // Update the rectangle.
                    switch (MouseHitType)
                    {
                        case HitType.Left:
                            new_x += offset_x;
                            new_width -= offset_x;
                            break;
                        case HitType.Right:
                            new_width += offset_x;
                            break;
                    }

                    // Don't use negative width or height.
                    if ((new_width > 0))
                    {


                        double distanceMoved = thisX - mouseDownX;
                        double pixelsPerSecond = 650 / Timelines[TimeLineIdx].Duration.TotalSeconds;

                        TimeSpan newDuration = TimeSpan.FromSeconds(new_width / pixelsPerSecond);
                        TimeSpan newStart = TimeSpan.FromSeconds(new_x / pixelsPerSecond);


                        // Update the rectangle.
                        Canvas.SetLeft(selectedRect, new_x);
                        selectedRect.Width = new_width;
                        Debug.WriteLine($"New width {newDuration}");
                        Debug.WriteLine($"New start {newStart}");

                        Timelines[TimeLineIdx].Events[TimeLineEventIdx].Start = newStart;
                        Timelines[TimeLineIdx].Events[TimeLineEventIdx].Duration = newDuration;
                        // Save the mouse's new location.
                        LastPoint = point;
                    }
                }
                else
                {
                    MouseHitType = SetHitType(selectedRect, rectParent);
                    SetMouseCursor();
                }
            }
            else if (MouseHitType == HitType.Body)
            {
                double thisX = Mouse.GetPosition(rectParent).X;
                MouseCurrentPosX = thisX;
                double distanceMoved = thisX - mouseDownX;
                double pixelsPerSecond = 650 / Timelines[TimeLineIdx].Duration.TotalSeconds;

                TimeSpan timeMoved = TimeSpan.FromSeconds(distanceMoved / pixelsPerSecond);

                MouseMovedInSeconds = timeMoved;
                //SelectedTimeLineEvent.Start = mouseDownStartTime + timeMoved;
                MouseDistanceMoved = distanceMoved;
                Timelines[TimeLineIdx].Events[TimeLineEventIdx].Start = mouseDownStartTime + timeMoved;
            }

            Timelines.Refresh();
        }

        /// <summary>
        /// When mouse leaves rectangle area
        /// </summary>
        /// <param name="rect"></param>
        /// <param name="args"></param>
        public void MouseOutsideTimelineEvent(object rect, MouseEventArgs args)
        {
            if (!(rect is Thumb selectedRect))
                return;

            Grid rectParent = selectedRect.Parent as Grid;

            MouseHitType = SetHitType(selectedRect, rectParent);
            SetMouseCursor();
        }

        #endregion

        #endregion

        #region Tests

        public HitType MouseHitType { get; set; }
        private bool DragInProgress = false;

        private Point LastPoint;

        public void MouseMoveTest(object rect, MouseEventArgs args)
        {
            if (!(rect is Thumb selectedRect))
                return;

            Grid rectParent = selectedRect.Parent as Grid;

            MouseHitType = SetHitType(selectedRect, rectParent);
            Debug.WriteLine($"MouseHitType: {MouseHitType}");
            SetMouseCursor();
        }

        public void MouseInsideElement(object rect, MouseEventArgs args)
        {
            if (!(rect is Rectangle selectedRect))
                return;


            Mouse.OverrideCursor = Cursors.ArrowCD;
        }

        public void MouseOutsideElement()
        {
            Mouse.OverrideCursor = Cursors.Arrow;

        }

        public void Cursor()
        {
            Cursor desired_cursor = Cursors.SizeWE;

            Mouse.OverrideCursor = desired_cursor;
        }

        public enum HitType
        {
            None,
            Body,
            Left,
            Right
        };

        public HitType SetHitType(Thumb rect, Grid rectParent)
        {
            Point p = rect.TranslatePoint(new Point(0, 0), rectParent);

            double left = p.X;
            Point point = Mouse.GetPosition(rectParent);
            double right = left + rect.Width;
            if (point.X < left) return HitType.None;
            if (point.X > right) return HitType.None;

            const double GAP = 10;
            if (point.X - left < GAP)
            {
                // Left edge.
                return HitType.Left;
            }
            else if (right - point.X < GAP)
            {
                // Right edge.
                return HitType.Right;
            }

            return HitType.Body;
        }

        //Set a mouse cursor appropriate for the current hit type.
        public void SetMouseCursor()
        {
            // See what cursor we should display.
            Cursor desired_cursor = Cursors.Arrow;
            switch (MouseHitType)
            {
                case HitType.None:
                    desired_cursor = Cursors.Arrow;
                    break;
                case HitType.Body:
                    desired_cursor = Cursors.Arrow;
                    break;
                case HitType.Left:
                case HitType.Right:
                    desired_cursor = Cursors.SizeWE;
                    break;
            }

            // Display the desired cursor.
            Mouse.OverrideCursor = desired_cursor;
        }


        #endregion

        #region Thumb

        public void OnDragStarted(object obj, EventArgs args)
        {
            if (!(obj is Thumb selectedThumb))
                return;
            selectedThumb.Background = Brushes.Orange;

        }

        public void OnDragCompleted(object obj, EventArgs args)
        {
            if (!(obj is Thumb selectedThumb))
                return;
            selectedThumb.Background = Brushes.AliceBlue;
            //Canvas.SetLeft(selectedThumb, 30);

        }

        public void OnDragDelta(object obj, DragDeltaEventArgs args)
        {

            //Debug.WriteLine(args.HorizontalChange);
            if (!(obj is Thumb selectedThumb))
                return;

            Grid parent = selectedThumb.Parent as Grid;


            TimeLineEvent clickedEvent = selectedThumb.DataContext as TimeLineEvent;

            MouseHitType = SetHitType(selectedThumb, parent);

            SetMouseCursor();

            //if (!LeftButtonDown)
            //    return;

            foreach (var timeline in Timelines)
            {
                foreach (var tlEvent in timeline.Events)
                {
                    if (tlEvent == clickedEvent)
                    {
                        SelectedTimeLineEvent = tlEvent;
                        TimeLineIdx = Timelines.IndexOf(timeline);
                        TimeLineEventIdx = timeline.Events.IndexOf(tlEvent);
                    }
                }
            }

            //DebugString = SelectedTimeLineEvent.Name;


            ////double thisX = Mouse.GetPosition(parent).X;
            ////MouseCurrentPosX = thisX;
            double distanceMoved = args.HorizontalChange;
            Debug.WriteLine($"Distance moved {distanceMoved} px");

            var posX = Canvas.GetLeft(selectedThumb);
            Point p = selectedThumb.TranslatePoint(new Point(0, 0), parent);
            Debug.WriteLine($"Pos {posX} px");

            Canvas.SetLeft(selectedThumb, posX + args.HorizontalChange);

            double pixelsPerSecond = 650 / Timelines[TimeLineIdx].Duration.TotalSeconds;

            TimeSpan timeMoved = TimeSpan.FromSeconds(distanceMoved / pixelsPerSecond);
            Debug.WriteLine($"Distance moved {timeMoved} seconds");

            MouseDistanceMoved = distanceMoved;
            MouseMovedInSeconds = timeMoved;
            //SelectedTimeLineEvent.Start = mouseDownStartTime + timeMoved;
            MouseDistanceMoved = distanceMoved;
            Debug.WriteLine($"Distance before {Timelines[TimeLineIdx].Events[TimeLineEventIdx].Start} seconds");

            //Timelines[TimeLineIdx].Events[TimeLineEventIdx].Start += timeMoved;

            Debug.WriteLine($"Distance after {Timelines[TimeLineIdx].Events[TimeLineEventIdx].Start} seconds");



            Timelines.Refresh();

        }


        #endregion

        #region Handlers

        /// <summary>
        /// CurrentTimeInvalidatedEventHandler's message
        /// </summary>
        /// <param name="message"></param>
        public void Handle(Clock message)
        {
            Debug.WriteLine($"Current time is: {message.CurrentTime}");
            CurrentTime = message.CurrentTime.ToString();
            var progress = message.CurrentProgress;
            Debug.WriteLine($"Progres: {progress * 100} % ");
            FloaterPos = Convert.ToInt32(progress * 650);
        }
        #endregion

    }
}
