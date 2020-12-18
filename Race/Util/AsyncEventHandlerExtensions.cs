using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Race.Util
{
    public delegate Task AsyncEventHandler(object sender, EventArgs e);
    public delegate Task AsyncEventHandler<TEventArgs>(object sender, TEventArgs e);

    /// <summary>
    /// Events to use with async event handlers
    /// https://stackoverflow.com/questions/12451609/how-to-await-raising-an-eventhandler-event/30739162
    /// this answer:
    /// https://stackoverflow.com/a/35280607
    /// </summary>
    public static class AsyncEventHandlerExtensions
    {
        // extensions for generic form
        public static IEnumerable<AsyncEventHandler> GetHandlers(this AsyncEventHandler handler)
        {
            return handler.GetInvocationList().Cast<AsyncEventHandler>();
        }

        public static Task InvokeAllAsync(this AsyncEventHandler handler, object sender, EventArgs e)
        {
            return Task.WhenAll(handler.GetHandlers().Select(handleAsync => handleAsync(sender, e)));
        }

        // extensions for template form
        public static IEnumerable<AsyncEventHandler<TEventArgs>> GetHandlers<TEventArgs>(this AsyncEventHandler<TEventArgs> handler)
        {
            return handler.GetInvocationList().Cast<AsyncEventHandler<TEventArgs>>();
        }

        public static Task InvokeAllAsync<TEventArgs>(this AsyncEventHandler<TEventArgs> handler, object sender, TEventArgs e)
        {
            return Task.WhenAll(handler.GetHandlers().Select(handleAsync => handleAsync(sender, e)));
        }
    }
}
