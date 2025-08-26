using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Security.Permissions;
using System.Text.RegularExpressions;

[Serializable]
public class BNumber : IComparable<BNumber>, IEquatable<BNumber>, ISerializable
{
    // 存储小数部分，保持在[1, 10)范围内
    private double _number;

    // 存储10的幂次数（位数）
    private int _digits;

    // 单位标记映射表：键为指数，值为单位符号
    private static readonly Dictionary<int, string> _unitMap;

    // 单位指数的反向映射：键为单位符号，值为指数
    private static readonly Dictionary<string, int> _unitExponentMap;

    // 用于解析的正则表达式
    private static readonly Regex _parseRegex =
        new Regex(@"^(?<number>-?\d+(\.\d+)?)(?<unit>[a-zA-Zz]+)?$", RegexOptions.Compiled);

    // 所有单位指数的有序列表，用于快速查找最合适的单位
    private static readonly List<int> _sortedExponents;

    // 精度常量，用于浮点数比较
    private const double Precision = 1e-12;

    // 最大支持的单位指数
    public static readonly int MaxUnitExponent;

    static BNumber()
    {
        _unitMap = new Dictionary<int, string>();
        _unitExponentMap = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

        // 1. 优先添加KMBT单位（指数3,6,9,12）
        AddUnit(3, "K");
        AddUnit(6, "M");
        AddUnit(9, "B");
        AddUnit(12, "T");

        // 2. 两位小写字母单位（AA-ZZ），指数从15开始，步长3
        // 确保优先级低于一位字母，范围：15 (AA) → 2040 (ZZ)（15 + 675*3 = 2040）
        for (char first = 'A'; first <= 'Z'; first++)
        {
            for (char second = 'A'; second <= 'Z'; second++)
            {
                int index = (first - 'A') * 26 + (second - 'A');
                int exponent = 15 + index * 3;
                AddUnit(exponent, $"{first}{second}");
            }
        }

        // 排序指数列表（确保查找时按优先级匹配）
        _sortedExponents = new List<int>(_unitMap.Keys);
        _sortedExponents.Sort();

        MaxUnitExponent = _sortedExponents.Count > 0 ? _sortedExponents[_sortedExponents.Count - 1] : 0;
    }

    // 添加单位到映射表的辅助方法
    private static void AddUnit(int exponent, string unit)
    {
        if (!_unitMap.ContainsKey(exponent))
            _unitMap[exponent] = unit;

        if (!_unitExponentMap.ContainsKey(unit))
            _unitExponentMap[unit] = exponent;
    }

    // 构造函数
    public BNumber(double number, int digits)
    {
        Normalize(ref number, ref digits);
        _number = number;
        _digits = digits;
    }

    // 从数值创建BNumber的工厂方法
    public static BNumber FromValue(double value)
    {
        if (Math.Abs(value) < Precision)
            return new BNumber(0, 0);

        int digits = 0;
        double number = value;

        // 标准化数值
        Normalize(ref number, ref digits);

        return new BNumber(number, digits);
    }

    // 从字符串解析BNumber的工厂方法（例如"100.00K"）
    public static BNumber Parse(string s)
    {
        if (string.IsNullOrWhiteSpace(s))
            throw new ArgumentNullException(nameof(s));

        Match match = _parseRegex.Match(s.Trim());
        if (!match.Success)
            throw new FormatException("无效的BNumber格式");

        string numberPart = match.Groups["number"].Value;
        string unitPart = match.Groups["unit"].Success ? match.Groups["unit"].Value : "";

        if (!double.TryParse(numberPart, out double number))
            throw new FormatException("无效的数字格式");

        int exponent = 0;
        if (!string.IsNullOrEmpty(unitPart) && !_unitExponentMap.TryGetValue(unitPart, out exponent))
            throw new FormatException($"未知的单位: {unitPart}");

        // 计算总指数
        int digits = exponent;

        // 标准化数值
        Normalize(ref number, ref digits);

        return new BNumber(number, digits);
    }

    // 用于反序列化的构造函数
    protected BNumber(SerializationInfo info, StreamingContext context)
    {
        _number = info.GetDouble(nameof(_number));
        _digits = info.GetInt32(nameof(_digits));
    }

    // 标准化方法，确保number在[1, 10)或(-10, -1]范围内
    private static void Normalize(ref double number, ref int digits)
    {
        if (Math.Abs(number) < Precision)
        {
            number = 0;
            digits = 0;
            return;
        }

        bool isNegative = number < 0;
        double absNumber = Math.Abs(number);

        // 调整到[1, 10)范围
        while (absNumber >= 10 - Precision)
        {
            absNumber /= 10;
            digits++;
        }

        while (absNumber < 1 && absNumber > Precision)
        {
            absNumber *= 10;
            digits--;
        }

        // 处理接近零的情况
        if (absNumber < Precision)
        {
            number = 0;
            digits = 0;
        }
        else
        {
            number = isNegative ? -absNumber : absNumber;
        }
    }

    // 实现IComparable<BNumber>接口
    public int CompareTo(BNumber other)
    {
        if (other == null) return 1;

        // 处理零的情况
        if (IsZero() && other.IsZero()) return 0;
        if (IsZero()) return -1;
        if (other.IsZero()) return 1;

        // 比较符号
        if (_number > 0 && other._number < 0) return 1;
        if (_number < 0 && other._number > 0) return -1;

        bool bothNegative = _number < 0 && other._number < 0;

        // 比较指数
        if (_digits != other._digits)
        {
            int comparison = _digits.CompareTo(other._digits);
            return bothNegative ? -comparison : comparison;
        }

        // 指数相同，比较数值部分
        int numComparison = _number.CompareTo(other._number);
        return bothNegative ? -numComparison : numComparison;
    }

    // 检查是否为零
    public bool IsZero()
    {
        return Math.Abs(_number) < Precision;
    }

    // 实现IEquatable<BNumber>接口
    public bool Equals(BNumber other)
    {
        if (other == null) return false;
        // 使用容差比较浮点数，避免精度问题
        return Math.Abs(_number - other._number) < Precision && _digits == other._digits;
    }

    // 覆写Equals方法
    public override bool Equals(object obj)
    {
        return Equals(obj as BNumber);
    }

    // 覆写GetHashCode方法
    public override int GetHashCode()
    {
        return HashCode.Combine(Math.Round(_number, 12), _digits);
    }

    // 实现ISerializable接口
    [SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
    public void GetObjectData(SerializationInfo info, StreamingContext context)
    {
        info.AddValue(nameof(_number), _number);
        info.AddValue(nameof(_digits), _digits);
    }

    // 【核心修复：默认ToString()自适应整数/小数】
    public override string ToString()
    {
        // 计算显示值，判断是否为整数
        int bestExponent = FindBestUnitExponent();
        double displayValue = _number * Math.Pow(10, _digits - bestExponent);
        displayValue = Math.Round(displayValue, 12);

        // 整数使用"0"格式，小数使用"0.00"格式
        return ToString(Math.Abs(displayValue - Math.Round(displayValue)) < Precision ? "0" : "0.00");
    }

    public string ToString(string format)
    {
        if (IsZero())
            return "0";

        int bestExponent = 0;
        if (_digits >= MaxUnitExponent)
        {
            bestExponent = MaxUnitExponent;
        }
        else
        {
            foreach (int exponent in _sortedExponents)
            {
                if (exponent <= _digits && exponent > bestExponent)
                    bestExponent = exponent;
            }

            if (bestExponent == 0 && _digits < 0)
            {
                foreach (int exponent in _sortedExponents)
                {
                    if (exponent >= _digits)
                    {
                        bestExponent = exponent;
                        break;
                    }
                }
            }
        }

        string bestUnit = bestExponent != 0 ? _unitMap[bestExponent] : "";
        double displayValue = _number * Math.Pow(10, _digits - bestExponent);
        displayValue = Math.Round(displayValue, 12);

        // 整数格式处理
        if (format == "0" && Math.Abs(displayValue - Math.Round(displayValue)) < Precision)
        {
            return $"{(long)Math.Round(displayValue)}{bestUnit}";
        }

        return $"{displayValue.ToString(format)}{bestUnit}";
    }

    // 数学方法
    public BNumber Sum(BNumber other)
    {
        if (other == null)
            return new BNumber(_number, _digits);

        // 处理零的情况
        if (IsZero())
            return new BNumber(other._number, other._digits);
        if (other.IsZero())
            return new BNumber(_number, _digits);

        // 对齐指数
        int maxDigits = Math.Max(_digits, other._digits);
        double num1 = _number * Math.Pow(10, _digits - maxDigits);
        double num2 = other._number * Math.Pow(10, other._digits - maxDigits);

        double sum = num1 + num2;
        return new BNumber(sum, maxDigits);
    }

    public BNumber Difference(BNumber other)
    {
        if (other == null)
            return new BNumber(_number, _digits);

        // 处理零的情况
        if (IsZero())
            return new BNumber(-other._number, other._digits);
        if (other.IsZero())
            return new BNumber(_number, _digits);

        // 对齐指数
        int maxDigits = Math.Max(_digits, other._digits);
        double num1 = _number * Math.Pow(10, _digits - maxDigits);
        double num2 = other._number * Math.Pow(10, other._digits - maxDigits);

        double diff = num1 - num2;
        return new BNumber(diff, maxDigits);
    }

    public BNumber Product(BNumber other)
    {
        if (other == null || IsZero() || other.IsZero())
            return new BNumber(0, 0);

        // 乘法计算
        double productNumber = _number * other._number;
        int productDigits = _digits + other._digits;

        // 确保结果正确标准化
        Normalize(ref productNumber, ref productDigits);

        return new BNumber(productNumber, productDigits);
    }

    public BNumber Quotient(BNumber other)
    {
        if (other == null)
            throw new ArgumentNullException(nameof(other));
        if (other.IsZero())
            throw new DivideByZeroException();

        double quotientNumber = _number / other._number;
        int quotientDigits = _digits - other._digits;

        // 确保结果正确标准化
        Normalize(ref quotientNumber, ref quotientDigits);

        return new BNumber(quotientNumber, quotientDigits);
    }

    public BNumber Pow(int exponent)
    {
        if (exponent == 0)
            return new BNumber(1, 0);
        if (IsZero())
            return new BNumber(0, 0);

        // 计算幂
        double powNumber = Math.Pow(Math.Abs(_number), exponent);
        int powDigits = _digits * exponent;

        // 恢复符号
        if (_number < 0 && exponent % 2 != 0)
        {
            powNumber = -powNumber;
        }

        // 确保结果正确标准化
        Normalize(ref powNumber, ref powDigits);

        return new BNumber(powNumber, powDigits);
    }

    // 运算符重载
    public static BNumber operator +(BNumber a, BNumber b)
    {
        if (a == null) return b != null ? new BNumber(b._number, b._digits) : null;
        if (b == null) return new BNumber(a._number, a._digits);
        return a.Sum(b);
    }

    public static BNumber operator -(BNumber a, BNumber b)
    {
        if (a == null) return b != null ? new BNumber(-b._number, b._digits) : null;
        if (b == null) return new BNumber(a._number, a._digits);
        return a.Difference(b);
    }

    public static BNumber operator *(BNumber a, BNumber b)
    {
        if (a == null || b == null) return new BNumber(0, 0);
        return a.Product(b);
    }

    public static BNumber operator /(BNumber a, BNumber b)
    {
        if (a == null) return new BNumber(0, 0);
        if (b == null) throw new ArgumentNullException(nameof(b));
        return a.Quotient(b);
    }

    public static bool operator ==(BNumber a, BNumber b)
    {
        if (ReferenceEquals(a, b)) return true;
        if (a is null || b is null) return false;
        return a.Equals(b);
    }

    public static bool operator !=(BNumber a, BNumber b)
    {
        return !(a == b);
    }

    public static bool operator <(BNumber a, BNumber b)
    {
        if (a == null) return b != null;
        return a.CompareTo(b) < 0;
    }

    public static bool operator >(BNumber a, BNumber b)
    {
        if (a == null) return false;
        return a.CompareTo(b) > 0;
    }

    public static bool operator <=(BNumber a, BNumber b)
    {
        if (a == null) return true;
        return a.CompareTo(b) <= 0;
    }

    public static bool operator >=(BNumber a, BNumber b)
    {
        if (a == null) return b == null;
        return a.CompareTo(b) >= 0;
    }

    // 隐式转换运算符
    public static implicit operator BNumber(double value)
    {
        return FromValue(value);
    }

// 取整方法：基于“带单位的显示值”而非内部标准化数值
    public BNumber Round(int decimals = 0)
    {
        if (IsZero())
            return new BNumber(0, 0);

        // 步骤1：找到当前数值最合适的显示单位（如K/M/B等）
        int bestExponent = FindBestUnitExponent();

        // 步骤2：计算“带单位的显示值”
        double displayValue = _number * Math.Pow(10, _digits - bestExponent);

        // 步骤3：对显示值进行四舍五入
        double roundedDisplay = Math.Round(displayValue, decimals);

        // 步骤4：转换回“实际值”并创建新BNumber（自动标准化）
        double actualValue = roundedDisplay * Math.Pow(10, bestExponent);
        return BNumber.FromValue(actualValue);
    }

    public BNumber Floor()
    {
        if (IsZero())
            return new BNumber(0, 0);

        int bestExponent = FindBestUnitExponent();
        double displayValue = _number * Math.Pow(10, _digits - bestExponent);
        double flooredDisplay = Math.Floor(displayValue);

        double actualValue = flooredDisplay * Math.Pow(10, bestExponent);
        return BNumber.FromValue(actualValue);
    }

    public BNumber Ceil()
    {
        if (IsZero())
            return new BNumber(0, 0);

        int bestExponent = FindBestUnitExponent();
        double displayValue = _number * Math.Pow(10, _digits - bestExponent);
        double ceiledDisplay = Math.Ceiling(displayValue);

        double actualValue = ceiledDisplay * Math.Pow(10, bestExponent);
        return BNumber.FromValue(actualValue);
    }

    // 辅助方法：找到当前数值最合适的显示单位对应的指数
    private int FindBestUnitExponent()
    {
        int bestExponent = 0;
        if (_digits >= BNumber.MaxUnitExponent)
        {
            bestExponent = BNumber.MaxUnitExponent;
        }
        else
        {
            foreach (int exponent in BNumber._sortedExponents)
            {
                if (exponent <= _digits && exponent > bestExponent)
                {
                    bestExponent = exponent;
                }
            }

            if (bestExponent == 0 && _digits < 0)
            {
                foreach (int exponent in BNumber._sortedExponents)
                {
                    if (exponent >= _digits)
                    {
                        bestExponent = exponent;
                        break;
                    }
                }
            }
        }

        return bestExponent;
    }

    // 静态方法减少GC
    public static void Sum(BNumber a, BNumber b, out BNumber result)
    {
        if (a == null && b == null)
        {
            result = null;
            return;
        }

        if (a == null)
        {
            result = new BNumber(b._number, b._digits);
            return;
        }

        if (b == null)
        {
            result = new BNumber(a._number, a._digits);
            return;
        }

        result = a.Sum(b);
    }

    public static void Difference(BNumber a, BNumber b, out BNumber result)
    {
        if (a == null)
        {
            result = b != null ? new BNumber(-b._number, b._digits) : null;
            return;
        }

        if (b == null)
        {
            result = new BNumber(a._number, a._digits);
            return;
        }

        result = a.Difference(b);
    }

    public static void Product(BNumber a, BNumber b, out BNumber result)
    {
        if (a == null || b == null || a.IsZero() || b.IsZero())
        {
            result = new BNumber(0, 0);
            return;
        }

        result = a.Product(b);
    }
}