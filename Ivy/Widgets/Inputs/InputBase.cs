using Ivy.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ivy.Widgets.Inputs
{
    public abstract record InputBase<T> : WidgetBase<InputBase<T>>, IInput<T>
    {

        /// <summary>Gets the current value.</summary>
        [Prop] public T Value { get; set; } = default!;

        protected Func<Event<IInput<T>, T>, ValueTask>? _userHandler;

        public Func<Event<IInput<T>, T>, ValueTask>? OnChange
        {
            get => async ev =>
                {
                    var validation = Validate();
                    if (!validation.IsValid)
                    {
                        Invalid = validation.ErrorMessage;
                    }
                    else
                    {
                        Invalid = null;
                    }
                    if (_userHandler != null)
                        await _userHandler(ev);

                };
            set => _userHandler = value;
        }

        /// <summary>Gets or sets whether the input is disabled.</summary>
        [Prop] public bool Disabled { get; set; }

        /// <summary>Gets or sets the validation error message.</summary>
        [Prop] public string? Invalid { get; set; }

        /// <summary>Gets or sets the event handler called when the input loses focus.</summary>
        [Event] public Func<Event<IAnyInput>, ValueTask>? OnBlur { get; set; }

        /// <summary>
        /// Returns the types that this input can bind to.
        /// </summary>
        public virtual Type[] SupportedStateTypes() => [];

        public virtual ValidationResult Validate()
        {
            return ValidationResult.Success();
        }
    }
}
