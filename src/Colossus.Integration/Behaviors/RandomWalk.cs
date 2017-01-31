namespace Colossus.Integration.Behaviors
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Colossus.Integration.Models;
    using Colossus.Integration.Processing;
    using Sitecore.Analytics;

    public class RandomWalk : SitecoreBehavior
    {
        public RandomWalk(string sitecoreUrl)
          : base(sitecoreUrl)
        {
        }

        protected override IEnumerable<Visit> Commit(SitecoreRequestContext ctx)
        {
            var visits = ctx.Visitor.GetVariable<double>("VisitCount", 1);
            var goalRoot = Sitecore.Configuration.Factory.GetDatabase("web").GetItem("{0CB97A9F-CAFB-42A0-8BE1-89AB9AE32BD9}");
            var goals = goalRoot.Axes.GetDescendants().Where(x => x.TemplateID == new Sitecore.Data.TemplateID(new Sitecore.Data.ID("{475E9026-333F-432D-A4DC-52E03B75CB6B}"))).ToArray();

            for (var i = 0; i < visits; i++)
            {
                double pause = 0;
                using (var visitContext = ctx.NewVisit())
                {
                    var outcomes = visitContext.Visit.GetVariable<IEnumerable<TriggerOutcomeData>>("TriggerOutcomes");
                    visitContext.Visit.Variables.Remove("TriggerOutcomes");

                    pause = visitContext.Visit.GetVariable<double>("Pause");

                    var landingPage = visitContext.Visit.GetVariable<string>("LandingPage");
                    if (string.IsNullOrEmpty(landingPage))
                    {
                        throw new Exception("Expected LandingPage");
                    }

                    var history = new List<string>();
                    var response = visitContext.Request(landingPage);
                    history.Add(landingPage);

                    var pageViews = (int)visitContext.Visit.GetVariable<double>("PageViews") - 1;

                    if (visitContext.Visit.GetVariable("Bounce", false))
                    {
                        pageViews = 1;
                    }


                    var internalSearchIndex = Randomness.Random.Next(0, pageViews);

                    for (var j = 0; j < pageViews; j++)
                    {
                        var nextUrl = this.GetNextUrl(response);
                        if (string.IsNullOrEmpty(nextUrl))
                        {
                            nextUrl = history.Count > 1 ? history[history.Count - 2] : history[0];
                        }
                        else
                        {
                            history.Add(nextUrl);
                        }


                        //Add outcomes to last visit
                        var variables = new Dictionary<string, object>();
                        if (j == pageViews - 1 && outcomes != null)
                        {
                            foreach (var oc in outcomes)
                            {
                                oc.DateTime = visitContext.Visit.End;
                            }
                            variables.Add("TriggerOutcomes", outcomes);
                        }

                        var events = new List<TriggerEventData>();
                        if (j == internalSearchIndex)
                        {
                            var internalKeywords = visitContext.Visit.GetVariable<string>("InternalSearch");
                            if (!string.IsNullOrEmpty(internalKeywords))
                            {
                                events.Add(new TriggerEventData
                                {
                                    Name = "Local search",
                                    Id = AnalyticsIds.SearchEvent.ToGuid(),
                                    Text = internalKeywords
                                });
                            }
                        }

                        var chance = Randomness.Random.NextDouble();
                        if (chance <= 0.03)
                        {
                            events.Add(new TriggerEventData
                            {
                                Name = "Register",
                                Id = Guid.Parse("{8FFB183B-DA1A-4C74-8F3A-9729E9FCFF6A}"),
                                Text = "User has registered"
                            });
                        }
                        else if (chance <= 0.1)
                        {
                            events.Add(new TriggerEventData
                            {
                                Name = "Login",
                                Id = Guid.Parse("{66722F52-2D13-4DCC-90FC-EA7117CF2298}"),
                                Text = "User has logged on"
                            });
                        }
                        else if (chance <= 0.16)
                        {
                            events.Add(new TriggerEventData
                            {
                                Name = "Call back Form Completed",
                                Id = Guid.Parse("{AC8E7BAB-0D60-4F48-9CE5-D988992BD95F}"),
                                Text = ""
                            });

                            if (Randomness.Random.Next(0, 1) <= 0.3)
                            {
                                events.Add(new TriggerEventData
                                {
                                    Name = "Call back received",
                                    Id = Guid.Parse("{DB1FCF73-1F18-4B32-8A35-8A101A7121D2}"),
                                    Text = ""
                                });
                            }
                        }
                        else if (chance <= 0.2)
                        {
                            events.Add(new TriggerEventData
                            {
                                Name = "Contact Us Form Completed",
                                Id = Guid.Parse("{A86BD826-6052-4844-8644-5D3EE18CE904}"),
                                Text = ""
                            });
                        }
                        else if (chance <= 0.25)
                        {
                            events.Add(new TriggerEventData
                            {
                                Name = "Filtered clients",
                                Id = Guid.Parse("{BFECC312-2844-479C-B330-8682FB25B4FE}"),
                                Text = ""
                            });
                        }
                        else if (chance <= 0.3)
                        {
                            events.Add(new TriggerEventData
                            {
                                Name = "Watched Video",
                                Id = Guid.Parse("{3382FA1A-FC67-4376-96F2-DA6A7D03E0AF}"),
                                Text = ""
                            });
                        }
                        else if (chance <= 0.34)
                        {
                            events.Add(new TriggerEventData
                            {
                                Name = "Downloaded Case Study",
                                Id = Guid.Parse("{208046A8-1581-4FA5-BEFD-4059A4EA5BFA}"),
                                Text = ""
                            });
                        }
                        else if (chance <= 0.36)
                        {
                            events.Add(new TriggerEventData
                            {
                                Name = "Global Newsletter Form Completed",
                                Id = Guid.Parse("{59B876EF-726A-454D-92A2-7A90E27FC9B2}"),
                                Text = ""
                            });
                        }
                        else if (chance <= 0.38)
                        {
                            var x = Randomness.Random.Next(0, goals.Length);
                            var goal = goals[x];
                        }


                        if (events.Count > 0)
                        {
                            variables.Add("TriggerEvents", events);
                        }

                        response = visitContext.Request(nextUrl, variables: variables);
                    }

                    yield return visitContext.Visit;
                }

                ctx.Pause(TimeSpan.FromDays(pause));
            }
        }

        protected virtual string GetNextUrl(SitecoreResponseInfo response)
        {
            var links = response.DocumentNode.SelectNodes("//a[@href]");
            if (links != null)
            {
                var localLinks =
                  links.Select(l => l.GetAttributeValue("href", "")).Where(l => l.StartsWith("/") && !l.EndsWith(".aspx")).ToArray();
                if (localLinks.Length > 0)
                {
                    return localLinks[Randomness.Random.Next(0, localLinks.Length)];
                }
            }

            return null;
        }
    }
}