﻿namespace ExperienceGenerator.Client.Models
{
    using System.Collections.Generic;

    public class ConfigurationOptions
    {
        public string Version { get; set; }
        public List<SelectionOption> Websites { get; set; }
        public List<SelectionOption> Languages { get; set; }
        public List<SelectionOptionGroup> LocationGroups { get; set; }
        public List<SelectionOption> Campaigns { get; set; }

        public List<SelectionOption> Goals { get; set; }
        public List<OptionTypes> ChannelTypes { get; set; }
        public List<SelectionOption> OrganicSearch { get; set; }
        public List<SelectionOption> PpcSearch { get; set; }
        public List<SelectionOptionGroup> OutcomeGroups { get; set; }
        public bool TrackerIsEnabled { get; set; }
    }
}