﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assisticant.Timers
{
    public class FloatingDateTime : IEquatable<FloatingDateTime>, IEquatable<DateTime>, IComparable<FloatingDateTime>, IComparable<DateTime>, IComparable
    {
        readonly FloatingTimeZone _zone;
        readonly TimeSpan _delta;

        public static FloatingDateTime UtcNow { get { return FloatingTimeZone.Utc.Now; } }
        public FloatingTimeZone Zone { get { return _zone; } }
        public TimeSpan FloatDelta { get { return _delta; } }
        public DateTime Snapshot { get { return _zone.GetStableTime() + _delta; } }
        public DateTime Date { get { return Floor(TimeSpan.FromDays(1)); } }
        public int Year { get { return GetComponent(Snapshot.Year, new DateTime(Snapshot.Year, 1, 1), new DateTime(Snapshot.Year + 1, 1, 1)); } }
        public int Month { get { return GetComponent(Snapshot.Month, new DateTime(Snapshot.Year, Snapshot.Month, 1), new DateTime(Snapshot.Year, Snapshot.Month, 1).AddMonths(1)); } }
        public int Day { get { return Date.Day; } }
        public int DayOfYear { get { return Date.DayOfYear; } }
        public DayOfWeek DayOfWeek { get { return Date.DayOfWeek; } }
        public int Hour { get { return Floor(TimeSpan.FromHours(1)).Hour; } }
        public int Minute { get { return Floor(TimeSpan.FromMinutes(1)).Minute; } }
        public int Second { get { return Floor(TimeSpan.FromSeconds(1)).Second; } }

        internal FloatingDateTime(FloatingTimeZone zone)
        {
            _zone = zone;
        }

        FloatingDateTime(FloatingTimeZone zone, TimeSpan delta)
        {
            _zone = zone;
            _delta = delta;
        }

        public FloatingDateTime Add(TimeSpan timespan) { return new FloatingDateTime(_zone, _delta + timespan); }
        public DateTime Add(DroppingTimeSpan timespan)
        {
            CheckZone(timespan);
            return timespan.ZeroMoment + _delta;
        }
        public FloatingDateTime AddDays(double days) { return Add(TimeSpan.FromDays(days)); }
        public FloatingDateTime AddHours(double hours) { return Add(TimeSpan.FromHours(hours)); }
        public FloatingDateTime AddMinutes(double minutes) { return Add(TimeSpan.FromMinutes(minutes)); }
        public FloatingDateTime AddSeconds(double seconds) { return Add(TimeSpan.FromSeconds(seconds)); }
        public FloatingDateTime AddMilliseconds(double milliseconds) { return Add(TimeSpan.FromMilliseconds(milliseconds)); }
        public FloatingDateTime AddTicks(long ticks) { return Add(TimeSpan.FromTicks(ticks)); }
        public FloatingDateTime Subtract(TimeSpan timespan) { return Add(-timespan); }
        public TimeSpan Subtract(FloatingDateTime other)
        {
            CheckZone(other);
            return _delta - other._delta;
        }
        public RisingTimeSpan Subtract(DateTime other) { return new RisingTimeSpan(_zone, other - _delta); }
        public DateTime Subtract(RisingTimeSpan timespan)
        {
            CheckZone(timespan);
            return timespan.ZeroMoment + _delta;
        }

        public static FloatingDateTime operator +(FloatingDateTime left, TimeSpan right) { return left.Add(right); }
        public static DateTime operator +(FloatingDateTime left, DroppingTimeSpan right) { return left.Add(right); }
        public static DateTime operator +(DroppingTimeSpan left, FloatingDateTime right) { return right.Add(left); }
        public static FloatingDateTime operator -(FloatingDateTime left, TimeSpan right) { return left.Subtract(right); }
        public static TimeSpan operator -(FloatingDateTime left, FloatingDateTime right) { return left.Subtract(right); }
        public static RisingTimeSpan operator -(FloatingDateTime left, DateTime right) { return left.Subtract(right); }
        public static DroppingTimeSpan operator -(DateTime left, FloatingDateTime right) { return new DroppingTimeSpan(right._zone, left - right._delta); }
        public static DateTime operator -(FloatingDateTime left, RisingTimeSpan right) { return left.Subtract(right); }

        public static bool operator ==(FloatingDateTime left, FloatingDateTime right) { return left.Equals(right); }
        public static bool operator !=(FloatingDateTime left, FloatingDateTime right) { return !(left == right); }
        public static bool operator <(FloatingDateTime left, FloatingDateTime right) { return left.CompareTo(right) < 0; }
        public static bool operator >(FloatingDateTime left, FloatingDateTime right) { return left.CompareTo(right) > 0; }
        public static bool operator <=(FloatingDateTime left, FloatingDateTime right) { return left.CompareTo(right) <= 0; }
        public static bool operator >=(FloatingDateTime left, FloatingDateTime right) { return left.CompareTo(right) >= 0; }

        public static bool operator ==(FloatingDateTime left, DateTime right) { return left.Equals(right); }
        public static bool operator ==(DateTime left, FloatingDateTime right) { return right == left; }
        public static bool operator !=(FloatingDateTime left, DateTime right) { return !(left == right); }
        public static bool operator !=(DateTime left, FloatingDateTime right) { return right != left; }
        public static bool operator >=(FloatingDateTime left, DateTime right) { return left.GetTimer(right).IsExpired; }
        public static bool operator >=(DateTime left, FloatingDateTime right) { return right <= left; }
        public static bool operator <(FloatingDateTime left, DateTime right) { return !(left >= right); }
        public static bool operator <(DateTime left, FloatingDateTime right) { return right > left; }
        public static bool operator <=(FloatingDateTime left, DateTime right) { return left < right || left == right; }
        public static bool operator <=(DateTime left, FloatingDateTime right) { return right >= left; }
        public static bool operator >(FloatingDateTime left, DateTime right) { return left >= right && left != right; }
        public static bool operator >(DateTime left, FloatingDateTime right) { return right < left; }

        public bool Equals(FloatingDateTime other) { return _zone == other._zone && _delta == other._delta; }
        public bool Equals(DateTime other) { return GetTimer(other).IsExpired && !GetTimer1(other).IsExpired; }
        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;
            if (obj is FloatingDateTime)
                return Equals((FloatingDateTime)obj);
            if (obj is DateTime)
                return Equals((DateTime)obj);
            throw new ArgumentException();
        }
        public override int GetHashCode() { return 31 * _zone.GetHashCode() + _delta.GetHashCode(); }

        public int CompareTo(FloatingDateTime other)
        {
            CheckZone(other);
            return _delta.CompareTo(other._delta);
        }
        public int CompareTo(DateTime other)
        {
            return !GetTimer(other).IsExpired ? -1 : GetTimer1(other).IsExpired ? 1 : 0;
        }
        public int CompareTo(object obj)
        {
            if (obj is FloatingDateTime)
                return CompareTo((FloatingDateTime)obj);
            if (obj is DateTime)
                return CompareTo((DateTime)obj);
            throw new ArgumentException();
        }

        public DateTime Floor(TimeSpan interval)
        {
            var floored = new DateTime(Snapshot.Ticks / interval.Ticks * interval.Ticks);
            return GetComponent(floored, floored, interval);
        }

        public override string ToString() { return Snapshot.ToString(); }

        ObservableTimer GetTimer(DateTime comparand) { return ObservableTimer.Get(_zone, comparand - _delta); }
        ObservableTimer GetTimer1(DateTime comparand) { return GetTimer(comparand.AddTicks(1)); }
        void CheckZone(FloatingDateTime other)
        {
            if (_zone != other._zone)
                throw new ArgumentException("Cannot relate ObservableDateTime from two different time zones");
        }
        void CheckZone(FloatingTimeSpan timespan)
        {
            if (_zone != timespan.Zone)
                throw new ArgumentException("Cannot relate ObservableDateTime to FloatingTimeSpan with different time zone");
        }
        T GetComponent<T>(T component, DateTime prev, TimeSpan increment)
        {
            return GetComponent(component, prev, prev + increment);
        }
        T GetComponent<T>(T component, DateTime prev, DateTime next)
        {
            ObservableTimer.Get(_zone, prev).OnGet();
            ObservableTimer.Get(_zone, next).OnGet();
            return component;
        }
    }
}
