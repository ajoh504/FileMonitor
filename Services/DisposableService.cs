namespace Services
{
    /// <summary>
    /// A class inheriting from <see cref="IDisposable"/>. Provides methods to properly dispose of any child service
    /// classes.
    /// </summary>
    public class DisposableService: IDisposable
    {
        private bool disposedValue;

        /// <summary>
        /// A virtual method. Override to ensure child classes are properly disposed.
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects)
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                disposedValue = true;
            }
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~DisposableService()
        // {
        //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        //     Dispose(disposing: false);
        // }

        /// <summary>
        /// The public <see cref="Dispose()"/> method which internally calls <see cref=
        /// "DisposableService.Dispose(bool)"/> and suppresses the object finalizer.
        /// </summary>
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
