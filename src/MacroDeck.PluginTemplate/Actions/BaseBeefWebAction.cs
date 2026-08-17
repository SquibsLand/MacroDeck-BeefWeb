using MacroDeck.Sdk.Actions;
using System;
using System.Collections.Generic;
using System.Text;

namespace MacroDeck.BeefWeb.Actions
{
    // TODO: Currently this only does logic for all keys having the same value, this should be changed.
    public abstract class BeefWebExecutor<T> : IActionExecutor
    {
        protected abstract BeefWebIntergration Intergration { get; }

        public abstract Task<ActionResult> ExecuteAsync(ActionExecutionContext context);
        protected static bool IsValid(ActionExecutionContext context, string key, out T? result)
        {
            context.Parameters.TryGetValue(key, out object? value);
            
            if (value is not null && value is T typedValue)
            {
                result = typedValue;
                return true;
            }

            result = default;
            return false;
        }
    }

    internal abstract class BaseBeefWebAction<TExecutor, T>(BeefWebIntergration integration) : IActionDefinition, IDynamicOptionsActionDefinition
        where TExecutor : BeefWebExecutor<T>
    {
        protected BeefWebIntergration Integration => integration;
        public abstract string Id { get; }
        public abstract string Name { get; }
        public abstract string Description { get; }
        public abstract IReadOnlyList<ActionParameter> Parameters { get; }

        protected abstract TExecutor CreateNewExecutor();
        IActionExecutor IActionDefinition.CreateExecutor() => CreateNewExecutor();


        public abstract Task<DynamicOptionsResult> GetDynamicOptionsAsync(DynamicOptionsContext context, CancellationToken cancellationToken);

  
    }
}
