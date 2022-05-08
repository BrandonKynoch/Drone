using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Utilities : MonoBehaviour {
    public static Utilities instance;

    private void Awake() {
        instance = this;
    }

    public static bool CloseEnough(Quaternion rotation, Quaternion desiredRotation, float offset) {
        if (Quaternion.Angle(rotation, desiredRotation) < offset)
            return true;
        return false;
    }

    public static bool CloseEnough(Vector3 vector, Vector3 desiredVector, float offset) {
        if (Vector3.Distance(vector, desiredVector) < offset)
            return true;
        return false;
    }

    public static bool CloseEnough(Vector2 vector, Vector2 desiredVector, float offset) {
        if (Vector2.Distance(vector, desiredVector) < offset)
            return true;
        return false;
    }

    public static bool CloseEnough(float val, float desiredVal, float offset) {
        if (val > desiredVal - offset && val < desiredVal + offset)
            return true;
        return false;
    }

    public static bool CloseEnough(Color col, Color desiredCol, float offset) {
        if (!Utilities.CloseEnough(col.r, desiredCol.r, offset))
            return false;
        if (!Utilities.CloseEnough(col.g, desiredCol.g, offset))
            return false;
        if (!Utilities.CloseEnough(col.b, desiredCol.b, offset))
            return false;
        if (!Utilities.CloseEnough(col.a, desiredCol.a, offset))
            return false;
        return true;
    }

    /// <summary>
    /// Finds closest degree while wrapping around 360
    /// </summary>
    public static bool CloseEnoughAngle(float rotation, float targetRotation, float offset) {
        float delta = Mathf.Abs(Mathf.DeltaAngle(rotation, targetRotation));
        if (delta < offset)
            return true;
        return false;
    }

    public static Vector3 AverageVector(Vector3[] vectors) {
        int count = vectors.Length;
        Vector3 sum = new Vector3();
        for (int i = 0; i < count; i++) {
            sum += vectors[i];
        }
        return sum / count;
    }

    public static float GetGreatestDistance(Vector3 start, Vector3[] vectors) {
        float max = 0f;
        foreach (Vector3 vec in vectors) {
            if (Vector3.Distance(start, vec) > max)
                max = Vector3.Distance(start, vec);
        }
        return max;
    }

    public static float GetShortestDistance(Vector3 start, Vector3[] vectors) {
        float min = float.PositiveInfinity;
        foreach (Vector3 vec in vectors) {
            if (Vector3.Distance(start, vec) < min)
                min = Vector3.Distance(start, vec);
        }
        return min;
    }

    public static float GetGreatestDistance(Vector3 start, Transform[] vectors) {
        float max = 0f;
        foreach (Transform vec in vectors) {
            try {
                if (Vector3.Distance(start, vec.position) > max)
                    max = Vector3.Distance(start, vec.position);
            } catch (System.NullReferenceException) { }
        }
        return max;
    }
    
    public static bool ArrayContains <T>(T[] arr, T element) {
        foreach (T c in arr) {
            if (c.Equals(element)) return true;
        }
        return false;
    }
    
    // Returns true if the two arrays intersect -> There is at least one value that is common in both arrays
    public static bool ArraysIntersect <T>(T[] a, T[] b) {
        foreach (T c in a) {
            if (ArrayContains(b, c)) return true;
        }
        return false;
    }

    public static bool ArrayTrue(bool[] boolean) {
        foreach (bool b in boolean) {
            if (!b)
                return false;
        }
        return true;
    }

    public static bool ArrayFalse(bool[] boolean) {
        foreach (bool b in boolean) {
            if (b)
                return false;
        }
        return true;
    }
    
    public static String ArrayToString <T>(T[] arr) {
        string output = "";
        for (int i = 0; i < arr.Length; i++) {
            output += arr[i].ToString();
            if (i < arr.Length - 1)
                output += ", ";
        }
        return output;
    }

    public static float[] BoolToFloatArray(bool[] data) {
        float[] floatData = new float[data.Length];
        for (int i = 0; i < data.Length; i++) {
            floatData[i] = (data[i]) ? 1f : 0f;
        }
        return floatData;
    }

    public static T[] ShiftArrayElementsRight<T>(T[] data) {
        if (data == null || data.Length == 0)
            return data;

        for (int i = data.Length - 1; i > 0; i--) {
            data[i] = data[i - 1];
        }
        data[0] = default(T);

        return data;
    }

    public static void FillArray<T>(ref T[] data, T target) {
        for (int i = 0; i < data.Length; i++) {
            data[i] = target;
        }
    }

    public static Vector3 ToVector3(Vector2 vector, bool horizontalPlane = false) {
        if (!horizontalPlane)
            return new Vector3(vector.x, vector.y, 0f);
        else
            return new Vector3(vector.x, 0f, vector.y);
    }

    public static bool RandomBool(float probability) {
        if (UnityEngine.Random.value < probability)
            return true;
        return false;
    }

    public static float ClosestMultiple(float x, float multiple) {
        float sign = Mathf.Sign(x);
        x = Mathf.Abs(x);
        multiple = Mathf.Abs(multiple);

        float mulMin = (x - (x % multiple));
        float mulMax = (x - (x % multiple)) + multiple;

        if (x - mulMin < mulMax - x) {
            return mulMin * sign;
        } else {
            return mulMax * sign;
        }
    }

    public static T SwapItems<T>(T list, int i, int ii) where T : IList {
        i = Mathf.Clamp(i, 0, list.Count - 1);
        ii = Mathf.Clamp(ii, 0, list.Count - 1);
        object TMP = list[i];
        list[i] = list[ii];
        list[ii] = TMP;
        return list;
    }

    public static int RoundUp(float value) {
        int nVal = Mathf.FloorToInt(value);
        if (nVal == value)
            return (int) value;
        return nVal + 1;
    }

    public static int Sum1ToN(int n) {
        return (n * (n + 1)) / 2;
    }

    public static int RandomInt(int start, int end) {
        return (int) UnityEngine.Random.Range(start, end + 0.9999f);
    }
    
    public static float WrapAround1(float f, bool exclude1 = false) {
        if (!exclude1) {
            if (f == (int) f)
                return f;
        }
        return f - ((int) f);
    }

    // Similar to bezier curve
    // Uses subdivision to smooth spline
    public static Vector3[] SubdivisionSmoothSpline(Vector3[] input, int subdivisions, float strength = 1f) {
        if (input.Length < 3)
            return input;

        strength = Mathf.Clamp01(strength);
        if (subdivisions < 1)
            subdivisions = 1;

        // Populate working data
        List<Vector3> data = new List<Vector3>();
        for (int i = 0; i < input.Length; i++) {
            data.Add(input[i]);
        }

        for (int subI = 0; subI < subdivisions; subI++) {
            // Subdivide
            for (int i = 0; i < data.Count - 1; i += 2) {
                Vector3 avg = (data[i] + data[i + 1]) / 2;
                data.Insert(i+1, avg);
            }

            // Smooth
            for (int i = 2; i < data.Count - 1; i += 2) {
                Vector3 avg = (data[i - 1] + data[i + 1]) / 2;
                Vector3 v = Vector3.Lerp(data[i], avg, (subI == subdivisions - 1)? strength: strength / 2);
                data[i] = v;
            }
        }

        return data.ToArray();
    }

    public static Vector2[] SubdivisionSmoothSpline(Vector2[] input, int subdivisions, float strength = 1f) {
        if (input.Length < 3)
            return input;

        strength = Mathf.Clamp01(strength);
        if (subdivisions < 1)
            subdivisions = 1;

        // Populate working data
        List<Vector2> data = new List<Vector2>();
        for (int i = 0; i < input.Length; i++) {
            data.Add(input[i]);
        }

        for (int subI = 0; subI < subdivisions; subI++) {
            // Subdivide
            for (int i = 0; i < data.Count - 1; i += 2) {
                Vector2 avg = (data[i] + data[i + 1]) / 2;
                data.Insert(i + 1, avg);
            }

            // Smooth
            for (int i = 2; i < data.Count - 1; i += 2) {
                Vector2 avg = (data[i - 1] + data[i + 1]) / 2;
                Vector2 v = Vector2.Lerp(data[i], avg, (subI == subdivisions - 1) ? strength : strength / 2);
                data[i] = v;
            }
        }

        return data.ToArray();
    }


    public static Vector2[] SlidingLinearSmoothSpline(Vector2[] input, int pointsCount = 0) {
        if (input.Length == 0) return null;
        if (pointsCount <= 0) pointsCount = input.Length;

        float deltaT = 1f / pointsCount;

        List<Vector2> output = new List<Vector2>();
        float t = 0;

        output.Add(input[0]);
        t += deltaT;

        while (t < 1f) {
            Vector2 lastPos = input[input.Length - 1];
            for (int i = input.Length - 2; i >= 0; i--) {
                lastPos = Vector2.Lerp(input[i], lastPos, t);
            }

            output.Add(lastPos);

            t += deltaT;
        }

        output.Add(input[input.Length-1]);

        return output.ToArray();
    }


    public static float ClampMin(float val, float minVal) {
        if (val > minVal)
            return val;
        return minVal;
    }

    public static int ClampMin(int val, int minVal) {
        if (val > minVal)
            return val;
        return minVal;
    }

    public static float ClampMax(float val, float maxVal) {
        if (val < maxVal)
            return val;
        return maxVal;
    }

    public static int ClampMax(int val, int maxVal) {
        if (val < maxVal)
            return val;
        return maxVal;
    }

    public static Vector2 ClampVector01(Vector2 v) {
        if (v.magnitude > 1f)
            return v.normalized;
        return v;
    }

    public static Vector3 ClampVector01(Vector3 v) {
        if (v.magnitude > 1f)
            return v.normalized;
        return v;
    }


    //##################################################################################################################################
    //######### Mono Utils #############################################################################################################
    //##################################################################################################################################

    public static void FadeUI(CanvasGroup canvasGroup, float alpha, float speed, bool enableDisable, Action onComplete = null) {
        instance.StartCoroutine(instance.Fadeui(canvasGroup, alpha, speed, enableDisable, onComplete));
    }

    private IEnumerator Fadeui(CanvasGroup canvasGroup, float alpha, float speed, bool enableDisable, Action onComplete) {
        if (canvasGroup == null)
            yield break;
        while (canvasGroup.GetComponent<FadingCheck>()) {
            Destroy(canvasGroup.GetComponent<FadingCheck>());
            yield return null;
            if (canvasGroup == null)
                yield break;
        }
        FadingCheck check = canvasGroup.gameObject.AddComponent<FadingCheck>();
        bool blocksRaycast = canvasGroup.blocksRaycasts;
        if (enableDisable) {
            canvasGroup.gameObject.SetActive(true);
        } else {
            canvasGroup.blocksRaycasts = false;
        }
        bool run = true;
        while (run && check != null) {
            canvasGroup.alpha = Mathf.Lerp(canvasGroup.alpha, alpha, speed * Time.deltaTime);
            if (Utilities.CloseEnough(canvasGroup.alpha, alpha, 0.005f)) {
                canvasGroup.alpha = alpha;
                run = false;
            }
            yield return null;
        }
        if (check != null && canvasGroup != null) {
            if (!enableDisable)
                canvasGroup.gameObject.SetActive(false);
            Destroy(canvasGroup.GetComponent<FadingCheck>());
        }
        if (onComplete != null)
            onComplete();
        if (blocksRaycast && canvasGroup != null)
            canvasGroup.blocksRaycasts = true;
        yield return null;
    }

    public static void FadeLineRenderer(LineRenderer lineR, float alpha, float speed, bool enableDisable, Action onComplete = null) {
        instance.StartCoroutine(instance.Fadelinerenderer(lineR, alpha, speed, enableDisable, onComplete));
    }

    private IEnumerator Fadelinerenderer(LineRenderer lineR, float alpha, float speed, bool enableDisable, Action onComplete) {
        speed = speed * 2;
        if (lineR.GetComponent<FadingCheck>())
            Destroy(lineR.GetComponent<FadingCheck>());
        FadingCheck check = lineR.gameObject.AddComponent<FadingCheck>();
        if (enableDisable)
            lineR.gameObject.SetActive(true);
        bool run = true;
        while (run && check != null) {
            Color startCol = lineR.startColor;
            startCol.a = Mathf.Lerp(startCol.a, alpha, speed * TimeHandler.CorrectedDeltaTime);
            Color endCol = lineR.endColor;
            endCol.a = Mathf.Lerp(endCol.a, alpha, speed * TimeHandler.CorrectedDeltaTime);
            if (Utilities.CloseEnough(startCol.a, alpha, 0.05f) && Utilities.CloseEnough(endCol.a, alpha, 0.05f)) {
                startCol.a = alpha;
                endCol.a = alpha;
                run = false;
            }
            lineR.startColor = startCol;
            lineR.endColor = endCol;
            yield return null;
        }
        if (check != null) {
            if (!enableDisable)
                lineR.gameObject.SetActive(false);
            Destroy(lineR.GetComponent<FadingCheck>());
        }
        if (onComplete != null)
            onComplete();
        yield return null;
    }

    public static void FadeLineRenderer(LineRenderer lineR, Color col, float speed, bool enableDisable, Action onComplete = null) {
        instance.StartCoroutine(instance.Fadelinerenderer(lineR, col, speed, enableDisable, onComplete));
    }

    private IEnumerator Fadelinerenderer(LineRenderer lineR, Color col, float speed, bool enableDisable, Action onComplete) {
        if (lineR.GetComponent<FadingCheck>())
            Destroy(lineR.GetComponent<FadingCheck>());
        FadingCheck check = lineR.gameObject.AddComponent<FadingCheck>();
        if (enableDisable)
            lineR.gameObject.SetActive(true);
        bool run = true;
        while (run && check != null) {
            lineR.startColor = Color.Lerp(lineR.startColor, col, speed * TimeHandler.CorrectedDeltaTime);
            lineR.endColor = Color.Lerp(lineR.endColor, col, speed * TimeHandler.CorrectedDeltaTime);

            if (Utilities.CloseEnough(lineR.startColor, col, 0.05f) && Utilities.CloseEnough(lineR.endColor, col, 0.05f)) {
                lineR.startColor = col;
                lineR.endColor = col;
                run = false;
            }
            yield return null;
        }
        if (check != null) {
            if (!enableDisable)
                lineR.gameObject.SetActive(false);
            Destroy(lineR.GetComponent<FadingCheck>());
        }
        if (onComplete != null)
            onComplete();
        yield return null;
    }

    public static void FadeSprite(GameObject obj, float alpha, float speed, Action callback = null) {
        instance.StartCoroutine(instance.FadeSpri(obj, alpha, speed, callback));
    }

    private IEnumerator FadeSpri(GameObject obj, float alpha, float speed, Action callback) {
        SpriteRenderer sr = obj.GetComponent<SpriteRenderer>();
        if (obj.GetComponent<FadingCheck>() != null) {
            Destroy(obj.GetComponent<FadingCheck>());
        }
        yield return null;
        if (obj == null)
            yield break;
        FadingCheck fc = obj.AddComponent<FadingCheck>();
        if (!obj.activeSelf) {
            obj.SetActive(true);
            Color col = sr.color;
            col.a = 0;
            sr.color = col;
        }
        while (sr != null && !Utilities.CloseEnough(sr.color.a, alpha, 0.01f) && fc != null) {
            Color col = sr.color;
            col.a = Mathf.Lerp(sr.color.a, alpha, speed * TimeHandler.CorrectedDeltaTime);
            sr.color = col;

            if (fc == null)
                yield break;
            yield return null;
        }
        if (sr != null && fc != null) {
            Color col = sr.color;
            col.a = alpha;
            sr.color = col;
            Destroy(fc);
        }
        try {
            if (callback != null && fc != null)
                callback();
        } catch { Debug.LogWarning("Callback Failed"); }
        yield break;
    }

    public static void FadeSprite(GameObject obj, Color col, float speed, Action callback = null) {
        instance.StartCoroutine(instance.FadeSpri(obj, col, speed, callback));
    }

    private IEnumerator FadeSpri(GameObject obj, Color newCol, float speed, Action callback) {
        SpriteRenderer sr = obj.GetComponent<SpriteRenderer>();
        FadingCheck fc = obj.GetComponent<FadingCheck>();
        if (fc != null) {
            Destroy(fc);
            yield return null;
        }
        yield return null;
        fc = obj.AddComponent<FadingCheck>();
        if (!obj.activeSelf) {
            obj.SetActive(true);
        }
        while (sr != null && !Utilities.CloseEnough(sr.color, newCol, 0.01f) && fc != null) {
            sr.color = Color.Lerp(sr.color, newCol, speed * TimeHandler.CorrectedDeltaTime); ;
            yield return null;
        }
        if (sr != null && fc != null) {
            sr.color = newCol;
            Destroy(fc);
        }
        try {
            if (callback != null && fc != null)
                callback();
        } catch { Debug.LogWarning("Callback Failed"); }
        yield break;
    }

    public static void FadeLight(Light light, float strength, float speed, bool enableDisable) {
        instance.StartCoroutine(instance.FadeL(light, strength, speed, enableDisable));
    }

    private IEnumerator FadeL(Light light, float strength, float speed, bool enableDisable) {
        if (light.GetComponent<FadingCheck>())
            Destroy(light.GetComponent<FadingCheck>());
        FadingCheck check = light.gameObject.AddComponent<FadingCheck>();
        if (enableDisable)
            light.gameObject.SetActive(true);

        bool run = true;
        while (run && check != null) {
            light.intensity = Mathf.Lerp(light.intensity, strength, speed * Time.deltaTime);
            if (Utilities.CloseEnough(light.intensity, strength, 0.05f)) {
                run = false;
                light.intensity = strength;
            }
            yield return null;
        }
        if (check != null) {
            if (!enableDisable)
                light.gameObject.SetActive(false);
            Destroy(light.GetComponent<FadingCheck>());
        }
        yield return null;
    }

    public static void FadeMat(Material mat, float alpha, float speed, Action callback = null) {
        instance.StartCoroutine(instance.FadeMatE(mat, alpha, speed, callback));
    }

    private IEnumerator FadeMatE(Material _mat, float alpha, float speed, Action callback) {
        while (!Utilities.CloseEnough(_mat.color.a, alpha, 0.01f)) {
            Color col = _mat.color;
            col.a = Mathf.Lerp(_mat.color.a, alpha, speed * TimeHandler.CorrectedDeltaTime);
            _mat.SetColor("_BaseColor", col);
            yield return null;
        }

        Color colF = _mat.color;
        colF.a = alpha;
        _mat.SetColor("_BaseColor", colF);

        if (callback != null)
            callback();
        yield break;
    }

    public static void YieldAction(Action action, float delay) {
        if (instance != null)
            instance.StartCoroutine(instance.yieldAction(action, delay));
    }

    private IEnumerator yieldAction(Action action, float delay) {
        if (delay > 0)
            yield return new WaitForSeconds(delay);
        else
            yield return null;
        if (action != null)
            action();
    }

    public static Vector2 RotateVector(Vector2 v, float a) {
        a *= Mathf.Deg2Rad;
        return new Vector2((v.x * Mathf.Cos(a)) - (v.y * Mathf.Sin(a)), (v.x * Mathf.Sin(a)) + (v.y * Mathf.Cos(a)));
    }








    public class CRandom {
        public class RandomInstance {
            private int randomVal = (int)(UnityEngine.Random.value * RANDOM_MAX);

            private const int RANDOM_MAX = 99999;
            private const int RANDOM_LENGTH = 5;

            private void GetNextVal() {
                randomVal = randomVal * randomVal;
                randomVal += (randomVal % 2) - (randomVal % 3);
                int excessDigits = randomVal.ToString().Length - RANDOM_LENGTH;
                if (excessDigits > 0) {
                    int startIndex = (excessDigits / 2) - 1;
                    if (startIndex < 0) startIndex = 0;
                    randomVal = int.Parse(randomVal.ToString().Substring(startIndex, RANDOM_LENGTH));
                }
            }

            public int Range(int min, int max) {
                GetNextVal();
                return (int) (((max - min) * Mathf.Abs((float) randomVal / RANDOM_MAX)) + min);
            }

            public float Range(float min, float max) {
                GetNextVal();
                return ((max - min) * Mathf.Abs((float) randomVal / RANDOM_MAX)) + min;
            }

            public bool Bool(float probability) {
                return Range(0f, 1f) < probability;
            }
        }
    }
}