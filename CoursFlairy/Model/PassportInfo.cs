using CoursFlairy.Model.Enum;

namespace CoursFlairy.Model
{
    public class PassportInfo
    {
        public Gender gender { get; set; }
        public string firstName { get; set; }
        public string lastName { get; set; }
        public DateTime birthDate { get; set; }
        public string passportNumber { get; set; }
        public DateTime passportDate { get; set; }
        public int CitizentshipID { get; set; }

        public PassportInfo(Gender gender, string firstName, string lastName, DateTime birthDate, string passportNumber, DateTime passportDate, int citizentshipID)
        {
            this.gender = gender;
            this.firstName = firstName;
            this.lastName = lastName;
            this.birthDate = birthDate;
            this.passportNumber = passportNumber;
            this.passportDate = passportDate;
            this.CitizentshipID = citizentshipID;
        }

        public override string ToString() => $"{firstName} {lastName} {birthDate} {gender} {passportNumber} {passportDate} {CitizentshipID}";
    }
}
