# Shimmy

## What is Shimmy?
Shimmy is an easy-to-use mocking framework based on [Pose](https://github.com/tonerdo/pose). Like Pose, Shimmy helps you test your code by replacing calls to methods (including static, non-virtual and even private methods) with delegates (shims). However, with Shimmy, you don't have to write these delegates yourself. 

Instead, simply point Shimmy at the method under test, and it will deconstruct said method and generate shims for any calls within it. Then, you can tell Shimmy how those delegates should behave. The work of writing shims is handled entirely by the library, leaving you free to focus on writing tests. Plus, Shimmy's delegates keep records of the parameters passed to them each time they're called, so you can use them to verify that your program is performing correctly. 

Like Pose, Shimmy is based on .NET Standard 2.0 and can be used across .NET platforms.

## How do I use Shimmy?

### First, create a PoseWrapper for your method

Include Shimmy in your project, and then, in your tests, ask Shimmy to build a Pose wrapper for your method under test by passing it an expression:
```c#
var wrapper = Shimmer.GetPoseWrapper(() => MyTestClass.AStaticMethod());
```

or a MethodInfo object:
```c#
var methodInfo = typeof(MyTestClass).GetMethod("AStaticMethod");
var wrapper = Shimmer.GetPoseWrapper(methodInfo);
```

or a delegate: 
```c#
var wrapper = Shimmer.GetPoseWrapper((Action)MyTestClass.AStaticMethod);
```

If your method has a return type, provide it as a generic parameter when using any of the three styles above:
```c#
var wrapper = Shimmer.GetPoseWrapper<int>(() => MyTestClass.MethodWithReturn());

var wrapper = Shimmer.GetPoseWrapper<int>((Func<int>)MyTestClass.MethodWithReturn);

var methodInfo = typeof(MyTestClass).GetMethod("MethodWithReturn");
var wrapper = Shimmer.GetPoseWrapper<int>(methodInfo);
```

If your method has parameters, and you are using the expression style, provide them with Pose.Is.A() or default():
```c#
var wrapper = Shimmer.GetPoseWrapper<int>(() => MyTestClass.MethodWithParametersAndReturn(Pose.Is.A<string>()));
var wrapper = Shimmer.GetPoseWrapper<int>(() => MyTestClass.MethodWithParametersAndReturn(default(string));
```

For instance methods, simply provide the instance:
```c#
var anInstance = new MyTestClass();
var wrapper = Shimmer.GetPoseWrapper<int>(() => anInstance.InstanceTestMethod());

var wrapper = Shimmer.GetPoseWrapper<int>((Func<int>)anInstance.InstanceTestMethod);

var methodInfo = typeof(MyTestClass).GetMethod("InstanceTestMethod");
var wrapper = Shimmer.GetPoseWrapper<int>(methodInfo, anInstance);
```

### Then, configure return values

Configure return values from methods within the method being tested:
```c#
wrapper.SetReturn(() => MyTestClass.AMethodCalledInMethodUnderTest()), value);
```
where value is the desired return value.

Return values can be set using expressions, MethodInfo objects, or delegates, using the same syntax as Shimmer.GetPoseWrapper() above. However, instances need not be specific, and may also use Pose.Is.A() or default().

### Execute your method through its wrapper

Simply call `wrapper.Execute`, passing parameters to be passed to your method:
```c#
var anInstance = new MyTestClass();
var wrapper = Shimmer.GetPoseWrapper<int>(() => anInstance.InstanceTestMethodWithIntParam(Pose.Is.A<int>()));
var result = wrapper.Execute(5);
```

### Review the results of method calls within the executed method

After a call to `wrapper.Execute`, details of all method calls made by the method under test during the last execution are stored in `wrapper.LastExecutionResults`. `LastExecutionResults` is a dictionary mapping `MethodInfo` objects to a list of `ShimmedMethodCall` objects.

Each `ShimmedMethodCall` object contains an array of `Parameters` (as `object[]`), and a `CalledAt` timestamp.

So, suppose `InstanceTestMethodWithIntParam` calls `MyTestClass.AStaticMethodWithStringReturn` twice. We would expect the following test to pass:
```c#
public void TestMethod() 
{
  var anInstance = new MyTestClass();
  var wrapper = Shimmer.GetPoseWrapper<int>(() => anInstance.InstanceTestMethodWithIntParam(Pose.Is.A<int>()));
  wrapper.SetReturn(() => MyTestClass.AStaticMethodWithStringReturn(Pose.Is.A<int>()), "bird");
  var result = wrapper.Execute(5);

  var lastExecutionResults = wrapper.LastExecutionResults;
  var resultsForMethod = wrapper.LastExecutionResults
              .First(ler => ler.Key.Name.Equals("AStaticMethodWithStringReturn"));
  Assert.AreEqual(2, resultsForMethod.Count);
  
  // let value1 be the parameter passed to the first call
  Assert.AreEqual(value1, (string)resultsForMethod[0].Parameters[0]);
  // let value2 be the parameter passed to the second call
  Assert.AreEqual(value2, (string)resultsForMethod[1].Parameters[0]);
  
  Assert.IsTrue(resultsForMethod[1].CalledAt > resultsForMethod[0].CalledAt);
}

Note that for getters and setters, when matching on method name (as opposed to method info), the name is prefixed with "get_" or "set_" for the respective action.

```

### Here's an example with it all together
```c#
class MyTestClass 
{
  public static string AStaticMethodWithStringReturn(int aParameter) 
  {
    ...
  }
  
  public int InstanceTestMethodWithIntParam(int theParameter) 
  {
    var stringValue1 = MyTestClass.AStaticMethodWithStringReturn(theParameter + 1);
    var stringValue2 = MyTestClass.AStaticMethodWithStringReturn(theParameter + 2);
    return stringValue1.Equals("bird") && stringValue2.Equals("bird")
      ? 12
      : 20;
  }
}

public void TestMethod() 
{
  var anInstance = new MyTestClass();
  var wrapper = Shimmer.GetPoseWrapper<int>(() => anInstance.InstanceTestMethodWithIntParam(Pose.Is.A<int>()));
  wrapper.SetReturn(() => MyTestClass.AStaticMethodWithStringReturn(Pose.Is.A<int>()), "bird");
  var result = wrapper.Execute(5);

  Assert.AreEqual(12, result);
  var lastExecutionResults = wrapper.LastExecutionResults;
  var resultsForMethod = wrapper.LastExecutionResults
            .First(ler => ler.Key.Name.Equals("AStaticMethodWithStringReturn"));
  Assert.AreEqual(2, resultsForMethod.Count);
  
  Assert.AreEqual(6, (string)resultsForMethod[0].Parameters[0]);
  Assert.AreEqual(7, (string)resultsForMethod[1].Parameters[0]);
  
  Assert.IsTrue(resultsForMethod[1].CalledAt > resultsForMethod[0].CalledAt);
}

```

## Configuration

The `WrapperOptions` enumeration provides options for configuring Shimmy's behavior:

| Option  | Default | Purpose |
| ------------- | ------------- | ------------- |
| ShimSpecialNames  | Disabled  | When enabled, shims will be generated for [special names](https://stackoverflow.com/questions/19788010/which-c-sharp-type-names-are-special) (excluding constructors) |
| ShimPrivateMembers | Disabled | When enabled, shims will be generated for private members  |
| ShimConstructors  | Enabled  | When enabled, shims will be generated for constructors (including private constructors) |

Default values are the values used when calling `Shimmer.GetPoseWrapper` without passing any WrapperOptions values. See `Shimmer.DefaultOptions`.

## Common Questions

### Can I use Shimmy side-by-side with Pose?

Shimmy includes a custom build of Pose (based on the latest Pose NuGet package, currently [1.2.1](https://github.com/tonerdo/pose/tree/86d63ba4857fc9dc6cba352e4be5b91b32707803)), so you cannot include both Shimmy and Pose's packages in the same project.

However, you can use the version of Pose packaged with Shimmy without using Shimmy, and existing code which uses Pose will continue to work if you swap out Pose's package for Shimmy's. Every effort has been made to keep the changes to Pose itself as limited as possible.

### Can I use Shimmy with another framework, like Moq?

Shimmy likely won't work well with classes and methods that are mocked by other frameworks. You may use them in the same class just fine, but they probably won't work together well if they have to share within that class.

Likewise, Shimmy cannot currently be used alongside handwritten Pose shims.

### When shouldn't I use Shimmy?

Shimmy, like Pose, is probably not fast enough or stable enough to use in non-testing code. Shimmy is also almost certainly slower than mocking frameworks which don't do method disassembly. 

Further, Shimmy is still alpha-level software.

## Roadmap

A complete roadmap with GitHub issues is coming soon. Short-term goals include:
* Simplifying syntax for interrogating last execution results
* Support for specifying custom return for shims based on:
  * How many times a method has been called
  * The values of the parameters passed to the method
* Improved support for shimming and testing getters and setters
* Support for shimming value-type properties

And longer-term goals:
* Addition of an instance value to ShimmedMethodCall
* Support for an "exclusion list" of methods for which shims should not be generated
* More tests and improved overall code quality
* Improvements to Pose's underlying behavior

## Contributions

I am happy to recieve pull requests and GitHub issues contributing code or making suggestions. Don't hesitate to get in touch!

## Credits, Contact and License

I am extremely grateful to the author of Pose, [Toni Solarin-Sodara](https://github.com/tonerdo), for his work, without which Shimmy would not be possible.

All other components of Shimmy were written by Maxwell Zimon. I can be contacted at https://zimon.co/contact.

Shimmy is released under the MIT license, as is Pose. See [Shimmy's license](https://github.com/frogsquire/shimmy/blob/master/LICENSE.md) and [Pose's license](https://github.com/frogsquire/shimmy/blob/master/POSELICENSE.md) for more details.
