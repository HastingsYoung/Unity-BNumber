# Unity-BNumber

Unity-BNumber是一个用于处理大数字及单位转换的工具类，特别适合游戏开发中需要展示和计算大额数值（如金币、经验值等）的场景。它支持自动转换为K（千）、M（百万）、B（十亿）、T（万亿）及扩展单位（a-z、aa-zz等），并提供完整的数学运算和格式化功能。


## 特性

- 自动处理大数字单位转换（K、M、B、T、a-z、aa-zz等）
- 支持基本数学运算（加减乘除、幂运算）
- 完善的比较操作（等于、不等于、大小比较）
- 灵活的格式化输出（自定义小数位数）
- 取整方法（四舍五入、向上取整、向下取整）
- 低GC设计，提供静态运算方法减少内存分配


## 安装

将`BNumber.cs`放入Unity项目的`Scripts`目录（或子目录）中即可使用。


## 基本使用

### 创建BNumber实例

```csharp
// 1. 使用构造函数（number: 1.23, digits: 10^5 → 1.23×10^5 = 123000 = 123K）
BNumber num1 = new BNumber(1.23, 5);

// 2. 从数值创建（100000 → 100K）
BNumber num2 = BNumber.FromValue(100000);

// 3. 从字符串解析（支持带单位的格式）
BNumber num3 = BNumber.Parse("2.5M");   // 2.5×10^6 = 2500000
BNumber num4 = BNumber.Parse("3.14B");  // 3.14×10^9 = 3140000000
```


## API 详细说明

### 1. 实例创建

| 方法 | 说明 | 示例 |
|------|------|------|
| `BNumber(double number, int digits)` | 构造函数，`number`为[1,10)范围的数值，`digits`为10的幂次数 | `new BNumber(1.23, 5)` → 1.23×10⁵ |
| `BNumber.FromValue(double value)` | 从数值创建实例（自动标准化） | `FromValue(123456)` → 123.46K |
| `BNumber.Parse(string s)` | 从字符串解析（支持单位） | `Parse("100.5K")` → 100.5×10³ |


### 2. 字符串输出

| 方法 | 说明 | 示例 |
|------|------|------|
| `ToString()` | 自动格式化（整数显示整数，小数显示两位小数） | `123000.ToString()` → "123K"；`123456.ToString()` → "123.46K" |
| `ToString(string format)` | 自定义格式（格式符同`double.ToString()`） | `123456.ToString("0.0")` → "123.5K"；`123456.ToString("0")` → "123K" |


### 3. 数学运算

| 运算 | 方法/运算符 | 说明 | 示例 |
|------|------------|------|------|
| 加法 | `+` 或 `Sum(BNumber other)` | 两数相加 | `num1 + num2`；`num1.Sum(num2)` |
| 减法 | `-` 或 `Difference(BNumber other)` | 两数相减 | `num1 - num2`；`num1.Difference(num2)` |
| 乘法 | `*` 或 `Product(BNumber other)` | 两数相乘 | `num1 * num2`；`num1.Product(num2)` |
| 除法 | `/` 或 `Quotient(BNumber other)` | 两数相除（除数不能为0） | `num1 / num2`；`num1.Quotient(num2)` |
| 幂运算 | `Pow(int exponent)` | 数值的n次幂 | `num1.Pow(2)` → num1的平方 |


### 4. 比较操作

| 操作 | 运算符/方法 | 说明 | 示例 |
|------|------------|------|------|
| 等于 | `==` 或 `Equals(BNumber other)` | 判断两数是否相等 | `num1 == num2`；`num1.Equals(num2)` |
| 不等于 | `!=` | 判断两数是否不等 | `num1 != num2` |
| 大于 | `>` | 判断当前数是否大于另一个数 | `num1 > num2` |
| 小于 | `<` | 判断当前数是否小于另一个数 | `num1 < num2` |
| 大于等于 | `>=` | 判断当前数是否大于等于另一个数 | `num1 >= num2` |
| 小于等于 | `<=` | 判断当前数是否小于等于另一个数 | `num1 <= num2` |


### 5. 取整方法

| 方法 | 说明 | 示例 |
|------|------|------|
| `Round(int decimals = 0)` | 四舍五入到指定小数位（基于显示值） | `123.456K.Round(2)` → 123.46K |
| `Floor()` | 向下取整（基于显示值） | `123.456K.Floor()` → 123K |
| `Ceil()` | 向上取整（基于显示值） | `123.456K.Ceil()` → 124K |


### 6. 静态方法（低GC）

| 方法 | 说明 | 示例 |
|------|------|------|
| `Sum(BNumber a, BNumber b, out BNumber result)` | 计算和，结果通过out参数返回 | `BNumber.Sum(a, b, out result)` |
| `Difference(BNumber a, BNumber b, out BNumber result)` | 计算差，结果通过out参数返回 | `BNumber.Difference(a, b, out result)` |
| `Product(BNumber a, BNumber b, out BNumber result)` | 计算积，结果通过out参数返回 | `BNumber.Product(a, b, out result)` |


### 7. 其他方法

| 方法 | 说明 | 示例 |
|------|------|------|
| `IsZero()` | 判断是否为零 | `num.IsZero()` → true/false |


## 单位体系

BNumber支持的单位及对应指数（10的幂）如下：

- 基础单位：K(10³)、M(10⁶)、B(10⁹)、T(10¹²)
- 一位小写字母：a(10¹⁵)、b(10¹⁸)、...、z(10⁹⁰)（步长3）
- 两位小写字母：aa(10⁹³)、ab(10⁹⁶)、...、zz(10²¹¹⁸)（步长3）

单位优先级：K/M/B/T > 一位小写字母 > 两位小写字母，转换时会自动选择最合适的单位。


## 测试

项目包含`BNumberTests.cs`测试类，涵盖以下场景：

- 对象创建与初始化
- 字符串解析正确性
- 数学运算准确性
- 比较操作逻辑
- 格式化输出效果
- 取整方法正确性
- 边界情况（零值、极值、除以零等）
- 单位优先级验证
- 性能测试（创建/运算效率）

可在Unity中运行测试场景查看结果。