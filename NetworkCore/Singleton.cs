namespace MNS
{
    public class Singleton<T> where T : class, new()
    {
        static T _Instance = null;

        static public T Instance
        {
            get
            {
                MakeInstance();
                return _Instance;
            }
        }

        static public void MakeInstance()
        {
            if (_Instance == null)
                _Instance = new T();
        }

        static public void ClearInstance()
        {
            if (_Instance != null)
                _Instance = null;
        }

        static public bool CheckInstance()
        {
            return _Instance != null;
        }
    }
}