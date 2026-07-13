using MelonLoader;

namespace Kebab_Mod_Menu_Count
{
    public static class ModSettings
    {
        private const string CategoryName = "KebabUnlimitedMenu";

        private static MelonPreferences_Category _category;
        private static MelonPreferences_Entry<bool> _menuUnlimited;
        private static MelonPreferences_Entry<int> _menuCustomValue;
        private static MelonPreferences_Entry<bool> _handUnlimited;
        private static MelonPreferences_Entry<int> _handCustomValue;

        private static MelonPreferences_Entry<int> _queuePointsCount;
        private static MelonPreferences_Entry<int> _groupSpawnPointsCount;
        private static MelonPreferences_Entry<int> _expectedCustomers;

        // "Active" values that the patches actually read
        public static int MaxMenuItems => _menuUnlimited.Value ? 999 : _menuCustomValue.Value;
        public static int MaxHandItems => _handUnlimited.Value ? 200 : _handCustomValue.Value;

        public static int QueuePointsCount
        {
            get => _queuePointsCount.Value;
            set { _queuePointsCount.Value = value; _category.SaveToFile(false); }
        }

        public static int GroupSpawnPointsCount
        {
            get => _groupSpawnPointsCount.Value;
            set { _groupSpawnPointsCount.Value = value; _category.SaveToFile(false); }
        }

        public static int ExpectedCustomers
        {
            get => _expectedCustomers.Value;
            set { _expectedCustomers.Value = value; _category.SaveToFile(false); }
        }

        public static bool MenuUnlimited
        {
            get => _menuUnlimited.Value;
            set { _menuUnlimited.Value = value; _category.SaveToFile(false); }
        }

        public static int MenuCustomValue
        {
            get => _menuCustomValue.Value;
            set { _menuCustomValue.Value = value; _category.SaveToFile(false); }
        }

        public static bool HandUnlimited
        {
            get => _handUnlimited.Value;
            set { _handUnlimited.Value = value; _category.SaveToFile(false); }
        }

        public static int HandCustomValue
        {
            get => _handCustomValue.Value;
            set { _handCustomValue.Value = value; _category.SaveToFile(false); }
        }

        public static void Init()
        {
            _category = MelonPreferences.CreateCategory(CategoryName, "Kebab Unlimited Menu");

            _menuUnlimited = _category.CreateEntry("MenuUnlimited", true, "Unlimited menu items");
            _menuCustomValue = _category.CreateEntry("MenuCustomValue", 10, "Custom menu item limit");

            _handUnlimited = _category.CreateEntry("HandUnlimited", true, "Unlimited hand items");
            _handCustomValue = _category.CreateEntry("HandCustomValue", 5, "Custom hand item limit");

            _queuePointsCount = _category.CreateEntry("QueuePointsCount", 200, "Queue points count");
            _groupSpawnPointsCount = _category.CreateEntry("GroupSpawnPointsCount", 50, "Group spawn points count");
            _expectedCustomers = _category.CreateEntry("ExpectedCustomers", 999, "Expected customers");
        }
    }
}