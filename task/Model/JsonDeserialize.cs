namespace task.Model.Model
{
    public class JsonDeserialize
    {
        public city[] city { get; set; }
    }

    public class city
    {
        public string id { get; set; }
        public string name { get; set; }
        public string code { get; set; }
        public int? cityID { get; set; }
        public string latitude { get; set; }
        public string longitude { get; set; }
        public string url { get; set; }
        public string timeshift { get; set; }
        public string requestEndTime { get; set; }
        public string sfrequestEndTime { get; set; }
        public string day2dayRequest { get; set; }
        public string day2daySFRequest { get; set; }
        public string preorderRequest { get; set; }
        public string freeStorageDays { get; set; }
        public terminals terminals { get; set; }
    }

    public class terminals
    {
        public terminal[] terminal { get; set; }
    }

    public class terminal
    {
        public string id { get; set; }
        public string name { get; set; }
        public string address { get; set; }
        public string fullAddress { get; set; }
        public string latitude { get; set; }
        public string longitude { get; set; }
        public bool isPVZ { get; set; }
        public bool cashOnDelivery { get; set; }
        public phone[] phones { get; set; }
        public bool storage { get; set; }
        public string mail { get; set; }
        public bool isOffice { get; set; }
        public bool receiveCargo { get; set; }
        public bool giveoutCargo { get; set; }
        public object maps { get; set; }
        public calcSchedule calcSchedule { get; set; }
        public bool @default { get; set; }
        public addressCode addressCode { get; set; }
        public string mainPhone { get; set; }
        public double maxWeight { get; set; }
        public double maxLength { get; set; }
        public double maxWidth { get; set; }
        public double maxHeight { get; set; }
        public double maxVolume { get; set; }
        public double maxShippingWeight { get; set; }
        public double maxShippingVolume { get; set; }
        public worktables worktables { get; set; }
    }

    public class calcSchedule
    {
        public string derival { get; set; }
        public string arrival { get; set; }
    }
    
    public class addressCode
    {
        public string street_code { get; set; }
    }
    
    public class phone {
        public string number { get; set; }
        public string type { get; set; }
        public string comment { get; set; }
        public bool primary { get; set; }
    }
    
    public class worktables
    {
        public specialWorktable specialWorktable { get; set; }
        worktable[] worktable { get; set; }
    }
    
    public class specialWorktable
    {
        public string[] receive { get; set; }
        public string[] giveout { get; set; }
    }
    
    public class worktable
    {
        public string department { get; set; }
        public string monday { get; set; }
        public string tuesday { get; set; }
        public string wednesday { get; set; }
        public string thursday { get; set; }
        public string friday { get; set; }
        public string saturday { get; set; }
        public string sunday { get; set; }
        public string timetable { get; set; }
    }

}
