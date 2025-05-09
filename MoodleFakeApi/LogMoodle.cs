namespace MoodleFakeApi
{
    public class LogMoodle
    {
        public string UserId { get; set; }
        public string Name { get; set; }
        public DateTime Date { get; set; }
        public string Action { get; set; }
        public string Target { get; set; }
        public string Component { get; set; }
        public string CourseFullname { get; set; }
        public DateTime UserLastAccess { get; set; }
    }
}
