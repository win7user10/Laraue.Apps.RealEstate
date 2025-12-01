namespace Laraue.Apps.RealEstate.Crawling.Impl;

[AttributeUsage(AttributeTargets.Class)]
public class JobGroupAttribute(string groupName) : Attribute
{
}