namespace AIChatServer.Entities.User
{
    public class UserData
    {
        public Guid Id { get; set; }
        public Gender Gender { get; set; }
        public string Name { get; set; }
        public int Age { get; set; }

        public bool IsFits(Preference preference)
        {
            if (IsGenderMatchs(Gender, preference.Gender) || preference.Gender==PreferenceGender.Any)
            {
                if (Age >= preference.MinAge && Age <= preference.MaxAge)
                {
                    return true;
                }
            }
            return false;
        }

        private bool IsGenderMatchs(Gender gender, PreferenceGender preferenceGender)
        {
            return preferenceGender switch
            {
                PreferenceGender.Any => true,
                PreferenceGender.Male => gender == Gender.Male,
                PreferenceGender.Female => gender == Gender.Female,
                _ => false
            };
        }

        public override string ToString()
        {
            return $"UserData {{{Id}}}\n{Gender}{Age}\n{Name}";
        }
    }
}
