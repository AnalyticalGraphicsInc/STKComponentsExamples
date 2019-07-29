// Create Cesium Viewer
var viewer = new Cesium.Viewer('cesiumContainer', {
    shouldAnimate: true
});
var baseLayerPickerViewModel = viewer.baseLayerPicker.viewModel;
var globe = viewer.scene.globe;

// If we can't access the default Cesium ion imagery (because we're offline) then fall back on Natural Earth imagery included in Cesium.
globe.imageryLayers.get(0).imageryProvider.readyPromise.otherwise(function(error) {
    console.log('Failed to load default imagery, switching to Natural Earth...');
    baseLayerPickerViewModel.selectedImagery = baseLayerPickerViewModel.imageryProviderViewModels.find(function(viewModel) {
        return /Natural Earth/.test(viewModel.name);
    });
});

// Enable terrain by default, unless we can't access it (because we're offline)
var defaultTerrainViewModel = baseLayerPickerViewModel.selectedTerrain; // WGS84 Ellipsoid
var cesiumWorldTerrainViewModel = baseLayerPickerViewModel.terrainProviderViewModels.find(function(viewModel) {
    return /Cesium World Terrain/.test(viewModel.name);
});
if (cesiumWorldTerrainViewModel !== undefined) {
    baseLayerPickerViewModel.selectedTerrain = cesiumWorldTerrainViewModel;
    globe.terrainProvider.readyPromise.otherwise(function(error) {
        console.log('Failed to load Cesium World Terrain, switching back to ellipsoid...');
        baseLayerPickerViewModel.selectedTerrain = defaultTerrainViewModel;
    });
}

// Create a simple View Model for the application.

// Note that this demonstration uses Knockout.js to create a user interface 
// because it is simple to use and included in Cesium already. 
// The capabilities being demonstrated can be used in any application framework.

var viewModel = {
    // default satellite identifier - International Space Station (ISS)
    satelliteIdentifier: '25544',
    generateEnabled: true,
    unloadEnabled: false,
    generate: function() {
        viewModel.generateEnabled = false;

        // Load CZML from the web service for the given satellite identifier.
        var url = 'GenerateCzml?id=' + viewModel.satelliteIdentifier;
        viewer.dataSources.add(Cesium.CzmlDataSource.load(url)).then(function(dataSource) {
            // The set of zoom targets is the set of entities with defined names.
            viewModel.zoomTargets = dataSource.entities.values.filter(function(entity) {
                return entity.name !== undefined;
            });
            viewModel.dataSource = dataSource;
            viewModel.unloadEnabled = true;
        });
    },
    unload: function() {
        viewer.dataSources.remove(viewModel.dataSource);
        viewModel.dataSource = undefined;
        viewModel.zoomTargets = [];
        viewModel.generateEnabled = true;
        viewModel.unloadEnabled = false;
    },
    zoomTargets: [],
    zoomTarget: undefined,
    zoom: function() {
        viewer.trackedEntity = viewModel.zoomTarget;
    }
};

// Bind the viewModel to the UI.
var toolbar = document.getElementById('toolbar');
Cesium.knockout.track(viewModel);
Cesium.knockout.applyBindings(viewModel, toolbar);
