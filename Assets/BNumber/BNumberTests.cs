using UnityEngine;
using System;
using System.Collections.Generic;
using Modules.Patterns;

public class BNumberTests : MonoBehaviour
{
    private List<string> testResults = new List<string>();
    private int passedTests = 0;
    private int failedTests = 0;

    // 单位换算常量
    private const long K = 1000; // 10^3
    private const long M = 1000000; // 10^6
    private const long B = 1000000000; // 10^9
    private const long T = 1000000000000; // 10^12

    void Start()
    {
        Debug.Log("开始BNumber类测试...");

        // 运行所有测试
        TestObjectCreation();
        TestStringParsing();
        TestMathematicalOperations();
        TestComparisonOperations();
        TestFormatting();
        TestRoundingMethods();
        TestEdgeCases();
        TestUnitPriority();
        TestPerformance();

        // 输出测试总结
        PrintTestSummary();
    }

    void OnGUI()
    {
        GUILayout.BeginArea(new Rect(20, 20, Screen.width - 40, Screen.height - 40));
        GUILayout.Label($"测试结果: 总计 {passedTests + failedTests}, 通过 {passedTests}, 失败 {failedTests}");
        GUILayout.Label("----------------------------------------");

        foreach (var result in testResults)
        {
            GUILayout.Label(result);
        }

        GUILayout.EndArea();
    }

    // 测试对象创建
    void TestObjectCreation()
    {
        LogTestStart("对象创建测试");

        try
        {
            // 使用构造函数创建 (1.23 × 10^5 = 123000 = 123K)
            BNumber num1 = new BNumber(1.23, 5);
            AssertEqual(num1.ToString(), "123K", "构造函数创建测试");

            // 使用FromValue创建 (100000 = 100K)
            BNumber num2 = BNumber.FromValue(100000);
            AssertEqual(num2.ToString(), "100K", "FromValue创建测试");

            // 测试零值
            BNumber zero = BNumber.FromValue(0);
            AssertEqual(zero.ToString(), "0", "零值创建测试");

            // 测试负值 (-5.67 × 10^3 = -5670 = -5.67K)
            BNumber negative = new BNumber(-5.67, 3);
            AssertEqual(negative.ToString(), "-5.67K", "负值创建测试");
        }
        catch (Exception ex)
        {
            LogTestFailure($"对象创建测试失败: {ex.Message}");
        }
    }

    // 测试字符串解析
    void TestStringParsing()
    {
        LogTestStart("字符串解析测试");

        try
        {
            BNumber num1 = BNumber.Parse("100.00K");
            AssertEqual(num1.ToString(), "100K", "K单位解析");

            BNumber num2 = BNumber.Parse("2.5M");
            AssertEqual(num2.ToString(), "2.50M", "M单位解析");

            BNumber num3 = BNumber.Parse("3.1415B");
            AssertEqual(num3.ToString("0.000"), "3.142B", "B单位解析与格式化");

            BNumber num4 = BNumber.Parse("123.45a");
            AssertEqual(num4.ToString(), "123.45a", "a单位解析");

            BNumber num5 = BNumber.Parse("-67.89zz");
            AssertEqual(num5.ToString(), "-67.89zz", "zz单位和负值解析");

            // 测试无效格式
            bool exceptionThrown = false;
            try
            {
                BNumber.Parse("invalid");
            }
            catch (FormatException)
            {
                exceptionThrown = true;
            }

            AssertTrue(exceptionThrown, "无效格式异常测试");
        }
        catch (Exception ex)
        {
            LogTestFailure($"字符串解析测试失败: {ex.Message}");
        }
    }

    // 测试数学运算
    void TestMathematicalOperations()
    {
        LogTestStart("数学运算测试");

        try
        {
            BNumber a = BNumber.Parse("100K"); // 100,000 = 100 × 10^3
            BNumber b = BNumber.Parse("200K"); // 200,000 = 200 × 10^3

            // 加法测试: 100K + 200K = 300K
            BNumber sum = a + b;
            AssertEqual(sum.ToString(), "300K", "加法测试");

            // 减法测试: 200K - 100K = 100K
            BNumber diff = b - a;
            AssertEqual(diff.ToString(), "100K", "减法测试");

            // 乘法测试: 100K × 200K = 20,000,000,000 = 20 × 10^9 = 20B
            // 计算过程: (100 × 10^3) × (200 × 10^3) = 20,000 × 10^6 = 20 × 10^9
            BNumber product = a * b;
            AssertEqual(product.ToString(), "20B", "乘法测试");

            // 除法测试: 200K ÷ 100K = 2
            BNumber quotient = b / a;
            AssertEqual(quotient.ToString(), "2", "除法测试");

            // 幂运算测试: (100K)^2 = 10,000,000,000 = 10 × 10^9 = 10B
            // 计算过程: (100 × 10^3)^2 = 10,000 × 10^6 = 10 × 10^9
            BNumber power = a.Pow(2);
            AssertEqual(power.ToString(), "10B", "幂运算测试");

            // 使用静态方法测试
            BNumber sumResult;
            BNumber.Sum(a, b, out sumResult);
            AssertEqual(sumResult.ToString(), "300K", "静态Sum方法测试");
        }
        catch (Exception ex)
        {
            LogTestFailure($"数学运算测试失败: {ex.Message}");
        }
    }

    // 测试比较操作
    void TestComparisonOperations()
    {
        LogTestStart("比较操作测试");

        try
        {
            BNumber a = BNumber.Parse("100K");
            BNumber b = BNumber.Parse("200K");
            BNumber c = BNumber.Parse("100K");

            // 相等性测试
            AssertTrue(a == c, "相等性测试1");
            AssertTrue(a.Equals(c), "相等性测试2");
            AssertTrue(a != b, "不等性测试");

            // 比较测试
            AssertTrue(a < b, "小于测试");
            AssertTrue(b > a, "大于测试");
            AssertTrue(a <= c, "小于等于测试");
            AssertTrue(b >= a, "大于等于测试");

            // 不同单位比较 (100K = 0.1M)
            BNumber d = BNumber.Parse("0.1M");
            AssertTrue(a == d, "不同单位相等性测试");
        }
        catch (Exception ex)
        {
            LogTestFailure($"比较操作测试失败: {ex.Message}");
        }
    }

    // 测试格式化
    void TestFormatting()
    {
        LogTestStart("格式化测试");

        try
        {
            // 123,456,789 = 123.456789 × 10^6 = 123.46M
            BNumber num = BNumber.Parse("123456789");
            AssertEqual(num.ToString(), "123.46M", "默认格式化测试");
            AssertEqual(num.ToString("0"), "123M", "整数格式化测试");
            AssertEqual(num.ToString("0.0000"), "123.4568M", "四位小数格式化测试");

            // 1,234 = 1.234 × 10^3 = 1.23K
            BNumber smallNum = BNumber.FromValue(1234);
            AssertEqual(smallNum.ToString(), "1.23K", "小数值格式化测试");

            BNumber largeNum = BNumber.Parse("987.654zz");
            AssertEqual(largeNum.ToString("0.0"), "987.7zz", "大单位格式化测试");
        }
        catch (Exception ex)
        {
            LogTestFailure($"格式化测试失败: {ex.Message}");
        }
    }

    // 测试取整方法
    void TestRoundingMethods()
    {
        LogTestStart("取整方法测试");

        try
        {
            BNumber num = BNumber.Parse("123.456K");

            BNumber rounded = num.Round(2);
            AssertEqual(rounded.ToString(), "123.46K", "Round方法测试1");

            BNumber rounded2 = num.Round(0);
            AssertEqual(rounded2.ToString(), "123K", "Round方法测试2");

            BNumber floored = num.Floor();
            AssertEqual(floored.ToString(), "123K", "Floor方法测试");

            BNumber ceiled = num.Ceil();
            AssertEqual(ceiled.ToString(), "124K", "Ceil方法测试");

            BNumber negativeNum = BNumber.Parse("-123.456K");
            BNumber negativeFloored = negativeNum.Floor();
            AssertEqual(negativeFloored.ToString(), "-124K", "负值Floor方法测试");
        }
        catch (Exception ex)
        {
            LogTestFailure($"取整方法测试失败: {ex.Message}");
        }
    }

    // 测试边界情况
    void TestEdgeCases()
    {
        LogTestStart("边界情况测试");

        try
        {
            // 零值运算测试
            BNumber zero = BNumber.FromValue(0);
            BNumber a = BNumber.Parse("100K");

            AssertEqual((a + zero).ToString(), "100K", "零值加法测试");
            AssertEqual((a * zero).ToString(), "0", "零值乘法测试");

            // 极小值测试 (指数-100的极小值不应该使用a或b单位)
            BNumber tiny = new BNumber(1.0, -100);
            AssertFalse(tiny.ToString().Contains("a") || tiny.ToString().Contains("b"), "极小值处理测试");
            AssertTrue(tiny.ToString().Length > 1, "极小值格式测试");

            // 极大值测试 (2000指数应该使用zz单位)
            BNumber huge = new BNumber(9.9, 2118);
            AssertTrue(huge.ToString().Contains("zz"), "极大值处理测试");
            Debug.Log(huge.ToString());

            // 除以零测试
            bool exceptionThrown = false;
            try
            {
                BNumber result = a / zero;
            }
            catch (DivideByZeroException)
            {
                exceptionThrown = true;
            }

            AssertTrue(exceptionThrown, "除以零异常测试");
        }
        catch (Exception ex)
        {
            LogTestFailure($"边界情况测试失败: {ex.Message}");
        }
    }

    // 新增：单位优先级测试
    void TestUnitPriority()
    {
        LogTestStart("单位优先级测试（KMBT > 一位小写 > 两位小写）");

        try
        {
            // 测试1：KMBT优先于小写字母
            BNumber kmbtPriority1 = new BNumber(1, 12); // T的指数是12
            AssertEqual(kmbtPriority1.ToString(), "1T", "T单位优先于小写字母测试");

            BNumber kmbtPriority2 = new BNumber(1, 14); // 14 < 15（a的指数），无对应小写单位，用T的下一级
            AssertEqual(kmbtPriority2.ToString(), "100T", "KMBT范围外但小于小写起始指数测试");

            // 测试2：一位小写字母优先于两位小写字母
            BNumber singleOverDouble1 = new BNumber(1, 90); // z的指数是90
            AssertEqual(singleOverDouble1.ToString(), "1z", "一位字母z优先于两位字母测试");

            BNumber singleOverDouble2 = new BNumber(1, 93); // aa的指数是93
            AssertEqual(singleOverDouble2.ToString(), "1aa", "两位字母aa在一位字母z之后测试");

            // 测试3：边界值验证
            BNumber boundary1 = new BNumber(1, 15); // a的指数
            AssertEqual(boundary1.ToString(), "1a", "一位字母起始边界测试");

            BNumber boundary2 = new BNumber(1, 87); // y的指数（15+24*3=87）
            AssertEqual(boundary2.ToString(), "1y", "一位字母中间值测试");

            BNumber boundary3 = new BNumber(1, 2118); // zz的指数
            AssertEqual(boundary3.ToString(), "1zz", "两位字母最大值测试");

            // 测试4：重叠指数验证（确保一位字母覆盖两位字母可能的重叠范围）
            BNumber overlapTest = new BNumber(1, 84); // x的指数（15+23*3=84）
            AssertEqual(overlapTest.ToString(), "1x", "一位字母与两位字母潜在重叠区测试");
        }
        catch (Exception ex)
        {
            LogTestFailure($"单位优先级测试失败: {ex.Message}");
        }
    }

    // 测试性能
    void TestPerformance()
    {
        LogTestStart("性能测试");

        try
        {
            int iterations = 100000;
            BNumber a = BNumber.Parse("123.45K");
            BNumber b = BNumber.Parse("67.89M");

            // 测试创建性能
            DateTime start = DateTime.Now;
            for (int i = 0; i < iterations; i++)
            {
                BNumber temp = new BNumber(1.23, i % 100);
            }

            TimeSpan createTime = DateTime.Now - start;

            // 测试运算性能
            start = DateTime.Now;
            for (int i = 0; i < iterations; i++)
            {
                BNumber result = a * b + a;
            }

            TimeSpan operationTime = DateTime.Now - start;

            // 测试静态方法（减少GC）性能
            BNumber resultOut;
            start = DateTime.Now;
            for (int i = 0; i < iterations; i++)
            {
                BNumber.Product(a, b, out resultOut);
                BNumber.Sum(resultOut, a, out resultOut);
            }

            TimeSpan staticMethodTime = DateTime.Now - start;
            
            
            // 测试运算性能2
            start = DateTime.Now;
            BigNumber aa = BigNumber.Parse("123.45K");
            BigNumber bb = BigNumber.Parse("67.89M");
            for (int i = 0; i < iterations; i++)
            {
                BigNumber result = aa * bb + aa;
            }

            TimeSpan oldOperationTime = DateTime.Now - start;

            LogTestInfo($"{iterations}次创建: {createTime.TotalMilliseconds:F2}ms");
            LogTestInfo($"{iterations}次运算: {operationTime.TotalMilliseconds:F2}ms");
            LogTestInfo($"{iterations}次静态方法运算: {staticMethodTime.TotalMilliseconds:F2}ms");
            LogTestInfo($"{iterations}次老方法运算: {oldOperationTime.TotalMilliseconds:F2}ms");

            // 简单性能断言（根据实际情况调整阈值）
            AssertTrue(createTime.TotalSeconds < 1, "创建性能测试");
            AssertTrue(operationTime.TotalSeconds < 1, "运算性能测试");
        }
        catch (Exception ex)
        {
            LogTestFailure($"性能测试失败: {ex.Message}");
        }
    }

    // 辅助方法：断言条件为假
    void AssertFalse(bool condition, string testName)
    {
        if (!condition)
        {
            LogTestPass($"{testName}");
        }
        else
        {
            LogTestFailure($"{testName}");
        }
    }

    // 辅助方法：记录测试开始
    void LogTestStart(string testName)
    {
        testResults.Add($"[{DateTime.Now:HH:mm:ss}] 开始测试: {testName}");
    }

    // 辅助方法：记录测试信息
    void LogTestInfo(string message)
    {
        testResults.Add($"  信息: {message}");
    }

    // 辅助方法：记录测试通过
    void LogTestPass(string message)
    {
        testResults.Add($"  ✅ 通过: {message}");
        Debug.Log($"  ✅ 通过: {message}");
        passedTests++;
    }

    // 辅助方法：记录测试失败
    void LogTestFailure(string message)
    {
        testResults.Add($"  ❌ 失败: {message}");
        Debug.Log($"  ❌ 失败: {message}");
        failedTests++;
    }

    // 断言两个值相等
    void AssertEqual(object actual, object expected, string testName)
    {
        if (actual.Equals(expected))
        {
            LogTestPass($"{testName} (实际: {actual}, 预期: {expected})");
        }
        else
        {
            LogTestFailure($"{testName} (实际: {actual}, 预期: {expected})");
        }
    }

    // 断言条件为真
    void AssertTrue(bool condition, string testName)
    {
        if (condition)
        {
            LogTestPass($"{testName}");
        }
        else
        {
            LogTestFailure($"{testName}");
        }
    }

    // 输出测试总结
    void PrintTestSummary()
    {
        testResults.Add("----------------------------------------");
        testResults.Add($"测试总结: 总计 {passedTests + failedTests} 个测试，" +
                        $"{passedTests} 个通过，{failedTests} 个失败");

        if (failedTests == 0)
        {
            testResults.Add("🎉 所有测试通过!");
        }
        else
        {
            testResults.Add("❌ 有测试失败，请检查问题。");
        }

        Debug.Log($"测试完成: 总计 {passedTests + failedTests}，通过 {passedTests}，失败 {failedTests}");
    }
}