# Unity-BNumber

Unity-BNumber is a utility class for handling large numbers and unit conversions, ideal for game development scenarios requiring display and calculation of large values (such as gold, experience points, etc.). It supports automatic conversion to K (thousand), M (million), B (billion), T (trillion), and extended units (a-z, aa-zz, etc.), along with comprehensive mathematical operations and formatting capabilities.


## Features

- Automatic large number unit conversion (K, M, B, T, a-z, aa-zz, etc.)
- Support for basic mathematical operations (addition, subtraction, multiplication, division, exponentiation)
- Comprehensive comparison operations (equality, inequality, magnitude comparison)
- Flexible formatting options (custom decimal places)
- Rounding methods (round, floor, ceiling)
- Low GC design with static operation methods to reduce memory allocation


## Installation

Place `BNumber.cs` in your Unity project's `Scripts` directory (or subdirectory) to start using it.


## Basic Usage

### Creating BNumber Instances

```csharp
// 1. Using constructor (number: 1.23, digits: 10^5 → 1.23×10^5 = 123000 = 123K)
BNumber num1 = new BNumber(1.23, 5);

// 2. Creating from a value (100000 → 100K)
BNumber num2 = BNumber.FromValue(100000);

// 3. Parsing from a string (supports unit formats)
BNumber num3 = BNumber.Parse("2.5M");   // 2.5×10^6 = 2500000
BNumber num4 = BNumber.Parse("3.14B");  // 3.14×10^9 = 3140000000
```


## API Detailed Description

### 1. Instance Creation

| Method | Description | Example |
|--------|-------------|---------|
| `BNumber(double number, int digits)` | Constructor where `number` is in the range [1,10) and `digits` is the power of 10 | `new BNumber(1.23, 5)` → 1.23×10⁵ |
| `BNumber.FromValue(double value)` | Creates an instance from a value (automatically normalized) | `FromValue(123456)` → 123.46K |
| `BNumber.Parse(string s)` | Parses from a string (supports units) | `Parse("100.5K")` → 100.5×10³ |


### 2. String Output

| Method | Description | Example |
|--------|-------------|---------|
| `ToString()` | Automatic formatting (integers show as integers, decimals show 2 decimal places) | `123000.ToString()` → "123K"; `123456.ToString()` → "123.46K" |
| `ToString(string format)` | Custom formatting (format specifiers same as `double.ToString()`) | `123456.ToString("0.0")` → "123.5K"; `123456.ToString("0")` → "123K" |


### 3. Mathematical Operations

| Operation | Method/Operator | Description | Example |
|-----------|-----------------|-------------|---------|
| Addition | `+` or `Sum(BNumber other)` | Adds two numbers | `num1 + num2`; `num1.Sum(num2)` |
| Subtraction | `-` or `Difference(BNumber other)` | Subtracts two numbers | `num1 - num2`; `num1.Difference(num2)` |
| Multiplication | `*` or `Product(BNumber other)` | Multiplies two numbers | `num1 * num2`; `num1.Product(num2)` |
| Division | `/` or `Quotient(BNumber other)` | Divides two numbers (divisor cannot be zero) | `num1 / num2`; `num1.Quotient(num2)` |
| Exponentiation | `Pow(int exponent)` | Raises the number to the nth power | `num1.Pow(2)` → square of num1 |


### 4. Comparison Operations

| Operation | Operator/Method | Description | Example |
|-----------|-----------------|-------------|---------|
| Equality | `==` or `Equals(BNumber other)` | Checks if two numbers are equal | `num1 == num2`; `num1.Equals(num2)` |
| Inequality | `!=` | Checks if two numbers are not equal | `num1 != num2` |
| Greater than | `>` | Checks if current number is greater than another | `num1 > num2` |
| Less than | `<` | Checks if current number is less than another | `num1 < num2` |
| Greater than or equal | `>=` | Checks if current number is greater than or equal to another | `num1 >= num2` |
| Less than or equal | `<=` | Checks if current number is less than or equal to another | `num1 <= num2` |


### 5. Rounding Methods

| Method | Description | Example |
|--------|-------------|---------|
| `Round(int decimals = 0)` | Rounds to the specified decimal places (based on display value) | `123.456K.Round(2)` → 123.46K |
| `Floor()` | Rounds down (based on display value) | `123.456K.Floor()` → 123K |
| `Ceil()` | Rounds up (based on display value) | `123.456K.Ceil()` → 124K |


### 6. Static Methods (Low GC)

| Method | Description | Example |
|--------|-------------|---------|
| `Sum(BNumber a, BNumber b, out BNumber result)` | Calculates sum, returns result via out parameter | `BNumber.Sum(a, b, out result)` |
| `Difference(BNumber a, BNumber b, out BNumber result)` | Calculates difference, returns result via out parameter | `BNumber.Difference(a, b, out result)` |
| `Product(BNumber a, BNumber b, out BNumber result)` | Calculates product, returns result via out parameter | `BNumber.Product(a, b, out result)` |


### 7. Other Methods

| Method | Description | Example |
|--------|-------------|---------|
| `IsZero()` | Checks if the number is zero | `num.IsZero()` → true/false |


## Unit System

BNumber supports the following units and their corresponding exponents (powers of 10):

- Basic units: K(10³), M(10⁶), B(10⁹), T(10¹²)
- Single lowercase letters: a(10¹⁵), b(10¹⁸), ..., z(10⁹⁰) (step 3)
- Two lowercase letters: aa(10⁹³), ab(10⁹⁶), ..., zz(10²¹¹⁸) (step 3)

Unit priority: K/M/B/T > single lowercase letters > two lowercase letters. The most appropriate unit is automatically selected during conversion.

## Testing

The project includes a test class `BNumberTests.cs` covering the following scenarios:

- Object creation and initialization
- String parsing accuracy
- Mathematical operation precision
- Comparison logic
- Formatting output effects
- Rounding method correctness
- Edge cases (zero values, extreme values, division by zero, etc.)
- Unit priority verification
- Performance testing (creation/operation efficiency)

Run the test scene in Unity to view results.