using UnityEngine;
using System.Collections;

public class Coordinate : object {
    public int x;
    public int y;

    public Coordinate(int _x, int _y) {
        x = _x;
        y = _y;
    }

    public Coordinate(Vector2 vector) {
        x = Mathf.RoundToInt(vector.x);
        y = Mathf.RoundToInt(vector.y);
    }

    public Vector2 ToVector() {
        return new Vector2(x, y);
    }

    public static float Distance(Coordinate a, Coordinate b) {
        return Vector2.Distance(a.ToVector(), b.ToVector());
    }

    public static float Angle(Coordinate a, Coordinate b) {
        return Vector2.Angle(a.ToVector(), b.ToVector());
    }

    public static float Angle(Coordinate a, Coordinate b, float normal) {
        float angle = Vector2.Angle(a.ToVector(), b.ToVector());
        return angle - normal;
    }

    public override bool Equals(object obj) {
        if (obj == null) {
            return false;
        }

        Coordinate c = obj as Coordinate;
        if ((object)c == null) {
            return false;
        }

        return (x == c.x) && (y == c.y);
    }

    public bool Equals(Coordinate c) {
        if ((object)c == null) {
            return false;
        }

        return (x == c.x) && (y == c.y);
    }

    public override int GetHashCode() {
        return x ^ y;
    }

    public static bool operator ==(Coordinate a, Coordinate b) {
        if (System.Object.ReferenceEquals(a, b)) {
            return true;
        }

        if (((object)a == null) || ((object)b == null)) {
            return false;
        }

        return a.x == b.x && a.y == b.y;
    }

    public static bool operator !=(Coordinate a, Coordinate b) {
        return !(a == b);
    }

    public static Coordinate operator +(Coordinate a, Coordinate b) {
        return new Coordinate(a.x + b.x, a.y + b.y);
    }

    public static Coordinate operator -(Coordinate a, Coordinate b) {
        return new Coordinate(a.x - b.x, a.y - b.y);
    }

    public static Coordinate operator *(Coordinate a, Coordinate b) {
        return new Coordinate(Mathf.RoundToInt(a.x * b.x), Mathf.RoundToInt(a.y * b.y));
    }

    /// <summary>
    /// Warning: Output is rounded value!
    /// </summary>
    public static Coordinate operator /(Coordinate a, Coordinate b) {
        return new Coordinate(Mathf.RoundToInt(a.x / b.x), Mathf.RoundToInt(a.y / b.y));
    }

    public override string ToString() {
        return (x.ToString() + ":" + y.ToString());
    }

    public static Coordinate Zero {
        get {
            return new Coordinate(0, 0);
        }
    }
}
