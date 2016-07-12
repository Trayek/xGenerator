﻿namespace AnalyticsUpdater.sitecore.admin
{
  using System;
  using System.Web.UI;
  using AnalyticsUpdater.Repositories;

  public class RebuildAnalyticsIndex : Page
  {
    protected void Page_Load(object sender, EventArgs e)
    {
      UpdateAnalyticsVisitsRepository.RebuildAnalyticsIndex();
      this.Response.Write("Done!");
    }
  }
}