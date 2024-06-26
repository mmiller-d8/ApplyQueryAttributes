using System;
namespace D8.Core.Extensions;

public enum DependencyInjectionLifetime
{
	Transient = 0,
	Scoped = 1,
	Singleton = 2
}

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public class DependencyInjectedAttribute : Attribute
{
	public DependencyInjectedAttribute()
	{

	}

	public DependencyInjectionLifetime Lifetime { get; set; } = DependencyInjectionLifetime.Transient;
}

