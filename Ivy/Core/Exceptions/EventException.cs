namespace Ivy.Core.Exceptions;

/// <summary>
/// Exception that wraps errors occurring during event handler execution.
/// </summary>
/// <param name="ex">The underlying exception that occurred during event handling.</param>
public class EventException(Exception ex) : Exception(null, ex);