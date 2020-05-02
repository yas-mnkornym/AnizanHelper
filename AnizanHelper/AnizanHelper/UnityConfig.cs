﻿using System;
using System.Net.Http;
using AnizanHelper.Models.DbSearch;
using AnizanHelper.Models.Updating;
using AnizanHelper.Services;
using Unity;
using Unity.Injection;

namespace AnizanHelper
{
	public class UnityConfig
	{
		public void RegisterTypes(IUnityContainer unityContainer)
		{
			unityContainer.RegisterSingleton<HttpClient>(
				new InjectionFactory(_ => new HttpClient()));

			// ------------------------------------------
			// Updating
			// ------------------------------------------
			unityContainer.RegisterType<IUpdateInfoRetreiver, WebUpdateInfoRetreiver>(
				new InjectionConstructor(
					new ResolvedParameter<HttpClient>(),
					new InjectionParameter(new Uri(Constants.UpdateInfoUrl))));

			unityContainer.RegisterSingleton<IUpdateManager, UpdateManager>();
			unityContainer.RegisterSingleton<IServiceManager, ServiceManager>();

			unityContainer.RegisterSingleton<ISearchManager, SearchManager>();

			// For test
			//unityContainer.RegisterType<IUpdateInfoRetreiver, DummyUpdateInfoRetreiver>(
			//	new InjectionConstructor(
			//		new InjectionParameter(new Version(int.MaxValue, int.MaxValue, int.MaxValue, int.MaxValue))));
		}
	}
}
