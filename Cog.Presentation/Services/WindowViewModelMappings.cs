﻿using System;
using System.Collections.Generic;
using SIL.Cog.Application.ViewModels;
using SIL.Cog.Presentation.Views;

namespace SIL.Cog.Presentation.Services
{
	public class WindowViewModelMappings : IWindowViewModelMappings
	{
		private readonly IDictionary<Type, Type> _mappings;

		/// <summary>
		/// Initializes a new instance of the <see cref="WindowViewModelMappings"/> class.
		/// </summary>
		public WindowViewModelMappings()
		{
			_mappings = new Dictionary<Type, Type>
			{
				{typeof(EditAffixViewModel), typeof(EditAffixDialog)},
				{typeof(EditVarietyViewModel), typeof(EditVarietyDialog)},
				{typeof(EditMeaningViewModel), typeof(EditMeaningDialog)},
				{typeof(RunStemmerViewModel), typeof(RunStemmerDialog)},
				{typeof(ProgressViewModel), typeof(ProgressDialog)},
				{typeof(NewSegmentMappingViewModel), typeof(NewSegmentMappingDialog)},
				{typeof(ExportSimilarityMatrixViewModel), typeof(ExportSimilarityMatrixDialog)},
				{typeof(ExportNetworkGraphViewModel), typeof(ExportNetworkGraphDialog)},
				{typeof(ExportHierarchicalGraphViewModel), typeof(ExportHierarchicalGraphDialog)},
				{typeof(EditNaturalClassViewModel), typeof(EditNaturalClassDialog)},
				{typeof(EditUnnaturalClassViewModel), typeof(EditUnnaturalClassDialog)},
				{typeof(AddUnnaturalClassSegmentViewModel), typeof(AddUnnaturalClassSegmentDialog)},
				{typeof(EditRegionViewModel), typeof(EditRegionDialog)},
				{typeof(FindViewModel), typeof(FindDialog)},
				{typeof(ImportTextWordListsViewModel), typeof(ImportTextWordListsDialog)},
				{typeof(ExportGlobalCorrespondencesChartViewModel), typeof(ExportGlobalCorrespondencesChartDialog)},
				{typeof(ExportSegmentFrequenciesViewModel), typeof(ExportSegmentFrequenciesDialog)},
				{typeof(SelectVarietiesViewModel), typeof(SelectVarietiesDialog)},
				{typeof(AboutViewModel), typeof(AboutDialog)},
				{typeof(SegmentMappingsChartViewModel), typeof(SegmentMappingsChartDialog)}
			};
		}


		/// <summary>
		/// Gets the window type based on registered ViewModel type.
		/// </summary>
		/// <param name="viewModelType">The type of the ViewModel.</param>
		/// <returns>The window type based on registered ViewModel type.</returns>
		public Type GetWindowTypeFromViewModelType(Type viewModelType)
		{
			return _mappings[viewModelType];
		}
	}
}
