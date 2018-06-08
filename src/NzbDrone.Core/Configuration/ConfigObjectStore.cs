using NzbDrone.Common.Serializer;
using NzbDrone.Core.Datastore.Events;
using NzbDrone.Core.Messaging.Events;

namespace NzbDrone.Core.Configuration
{
    /// <summary>
    /// Similar in idea to <see cref="IBasicRepository{TModel}"/>, but with either 0 or 1 object that is JSON serialized
    /// and stored in the config table.
    /// </summary>
    /// <typeparam name="T">The type of object you wish to persist</typeparam>
    public interface IConfigObjectStore<T> where T : class, new()
    {
        /// <summary>
        /// <para>Getter - Gets the object from memory, or null if it doesn't exist.</para>
        /// <para>Setter - Sets the in-memory object and persists it to database, or erasing it if null.</para>
        /// </summary>
        T Item { get; set; }

        /// <summary>
        /// Deletes the object from the database. Equivalent to setting 'Item' to null.
        /// </summary>
        void Erase();

        /// <summary>
        /// Tries loading the object from the database into memory. Automatically called when
        /// the store is initialized.
        /// </summary>
        /// <returns>Whether the object was loaded/existed in the database</returns>
        bool Load();

        /// <summary>
        /// Persists the in-memory object to database. If it's null, it will be erased.
        /// </summary>
        void Save();

        /// <summary>
        /// Should model events(<see cref="ModelEvent{TModel}"/>) be published when creating, updating or deleting the Item?
        /// </summary>
        bool PublishModelEvents { get; }
    }

    public abstract class ConfigObjectStore<T> : IConfigObjectStore<T> where T : class, new()
    {
        private readonly IConfigRepository repo;
        private readonly IEventAggregator eventAggregator;

        private T _object = null;
        public ConfigObjectStore(IConfigRepository repo, IEventAggregator eventAggregator)
        {
            this.repo = repo;
            this.eventAggregator = eventAggregator;
            Load();
        }

        /// <summary>
        /// A unique database identifier for the object.
        /// </summary>
        protected abstract string ConfigKey { get; }

        public virtual bool PublishModelEvents => false;

        public T Item
        {
            get
            {
                return _object;
            }
            set
            {
                _object = value;
                if (_object == null)
                {
                    Erase();
                } else
                {
                    Save();
                }
            }
        }

        public bool Load()
        {
            var cfgItem = repo.Get(ConfigKey);
            if (cfgItem != null)
            {
                _object = Json.Deserialize<T>(cfgItem.Value);
                return true;
            }
            return false;
        }

        public void Save()
        {
            if (_object == null)
            {
                Erase();
                return;
            }
            var cfgItem = repo.Get(ConfigKey);
            if (cfgItem == null)
            {
                cfgItem = repo.Insert(new Config()
                {
                    Key = ConfigKey,
                    Value = _object.ToJson()
                });
                PublishModelEvent(_object, ModelAction.Created);
            } else
            {
                cfgItem.Value = _object.ToJson();
                repo.Update(cfgItem);
                PublishModelEvent(_object, ModelAction.Updated);
            }
        }

        public void Erase()
        {
            _object = null;
            var cfgItem = repo.Get(ConfigKey);
            if (cfgItem != null)
            {
                repo.Delete(cfgItem);
                PublishModelEvent(new T(), ModelAction.Deleted);
            }
        }

        private void PublishModelEvent(T model, ModelAction action)
        {
            if (PublishModelEvents)
            {
                eventAggregator.PublishEvent(new ModelEvent<T>(model, action));
            }
        }

    }
}
