using RadiantConnect;
using RadiantConnect.Network.PreGameEndpoints.DataTypes;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using static RadiantConnect.ValorantApi.Agents;
using static RadiantConnect.ValorantApi.Maps;

namespace ValoLocker;

public partial class MainWindow : Window
{
    private bool isGameOpen = false;

    private bool toggled = true;

    private ObservableCollection<AgentButton> agentsCollection = [];

    private Dictionary<string, string> selectedAgents = [];

    private ObservableCollection<MapButton> mapsCollection = [];

    private string selectedMap = "";

    public class AgentButton(string displayName, string displayIcon, string uuid) : INotifyPropertyChanged
    {
        public string DisplayName { get; set; } = displayName;
        public string DisplayIcon { get; set; } = displayIcon;
        public string Uuid { get; set; } = uuid;
        public bool IsChecked { get => field; set { field = value; OnPropertyChanged(nameof(IsChecked)); } } = false;

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public class MapButton(string displayName, string agentDisplayName, string uuid, bool isListedFirst = false, Visibility visibility = Visibility.Visible) : INotifyPropertyChanged
    {
        public string DisplayName { get; set; } = displayName;
        public string AgentDisplayName { get => field; set { field = value; OnPropertyChanged(nameof(AgentDisplayName)); } } = agentDisplayName;
        public string Uuid { get; set; } = uuid;
        public bool IsListedFirst { get; set; } = isListedFirst;
        public Visibility Visibility { get; set; } = visibility;

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public MainWindow()
    {
        InitializeComponent();

        agents.ItemsSource = agentsCollection;
        CollectionViewSource.GetDefaultView(agents.ItemsSource).SortDescriptions.Add(new SortDescription(nameof(AgentButton.DisplayName), ListSortDirection.Ascending));
        maps.ItemsSource = mapsCollection;
        CollectionViewSource.GetDefaultView(maps.ItemsSource).SortDescriptions.Add(new SortDescription(nameof(MapButton.IsListedFirst), ListSortDirection.Descending));
        CollectionViewSource.GetDefaultView(maps.ItemsSource).SortDescriptions.Add(new SortDescription(nameof(MapButton.DisplayName), ListSortDirection.Ascending));

        MapDropdownPopup.PlacementTarget = MapDropdownButton;

        Task.Run(async () =>
        {
            AgentsData agentsData;
            while (true)
            {
                AgentsData? result = await GetAgentsAsync();
                if (result != null && result.Data != null)
                {
                    agentsData = result;
                    break;
                }
                await Task.Delay(5000);
            }

            Application.Current.Dispatcher.Invoke(() =>
            {
                foreach (AgentData agentData in agentsData.Data)
                {
                    agentsCollection.Add(new(agentData.DisplayName, agentData.DisplayIcon, agentData.Uuid));
                }
            });

            MapsData mapsData;
            while (true)
            {
                MapsData? result = await GetMapsAsync();
                if (result != null && result.Data != null)
                {
                    mapsData = result;
                    break;
                }
                await Task.Delay(5000);
            }

            Application.Current.Dispatcher.Invoke(() =>
            {
                mapsCollection.Add(new("Default", "-", "", true));
                foreach (MapDatum mapData in mapsData.Data)
                {
                    if (mapData.PremierBackgroundImage != null)
                    {
                        mapsCollection.Add(new(mapData.DisplayName, "-", mapData.MapUrl));
                    }
                }
            });

            while (true)
            {
                isGameOpen = RadiantConnect.Utilities.InternalValorantMethods.ClientIsReady();
                Application.Current.Dispatcher.Invoke(() =>
                {
                    labelStatus.Content = isGameOpen ? "Valorant is running." : "Valorant not open...";
                });
                await Task.Delay(5000);
            }
        });

        Task.Run(async () =>
        {
            while (true)
            {
                while (!isGameOpen)
                {
                    await Task.Delay(5000);
                }

                Initiator initiator = new();

                initiator.TcpEvents.OnGameStateChanged += async (value) =>
                {
					if (value != "PREGAME" || !toggled) return;

                    PreGameMatch? preGameMatch = null;
                    try
                    {
                        preGameMatch = await initiator.Endpoints.PreGameEndpoints.FetchPreGameMatchAsync();
                    }
                    catch
                    {

                    }

                    if (preGameMatch == null) return;

                    if ((!selectedAgents.TryGetValue(preGameMatch.MapId, out string? agentId) || string.IsNullOrEmpty(agentId)) && !selectedAgents.TryGetValue("", out agentId))
                    {
                        return;
                    }

                    try
                    {
                        await Task.Delay(3000);
                        if (!toggled) return;

                        await initiator.ExternalSystem.Net.PostAsync<PreGameMatch>(initiator.ExternalSystem.ClientData.GlzUrl, $"/pregame/v1/matches/{preGameMatch.Id}/select/{agentId}");
                        await initiator.ExternalSystem.Net.PostAsync<PreGameMatch>(initiator.ExternalSystem.ClientData.GlzUrl, $"/pregame/v1/matches/{preGameMatch.Id}/lock/{agentId}");
                    }
                    catch
                    {
                        
                    }

                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        labelStatus.Content = "Locked agent.";
                    });
                };

                while (isGameOpen)
                {
                    await Task.Delay(5000);
                }

                Application.Current.Dispatcher.Invoke(() =>
                {
                    labelStatus.Content = "client no ready restart";
                });
            }
        });
    }

    private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) => DragMove();

    private void btnMinimize_Click(object sender, RoutedEventArgs e) => WindowState = WindowState.Minimized;

    private void btnClose_Click(object sender, RoutedEventArgs e) => Close();

    private void btnToggle_Click(object sender, RoutedEventArgs e)
    {
        toggled = !toggled;
        rectToggle.Fill = toggled ? new SolidColorBrush(Color.FromRgb(0x1F, 0xE8, 0xAD)) : new SolidColorBrush(Color.FromRgb(0xFF, 0x3F, 0x3F));
    }

    private void AgentButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is RadioButton radioButton && radioButton.DataContext is AgentButton agentButton)
        {
            if (selectedAgents.ContainsKey(selectedMap) && selectedAgents[selectedMap] == agentButton.Uuid)
            {
                selectedAgents[selectedMap] = string.Empty;
                labelSelectedMap2.Content = "-";
                mapsCollection.FirstOrDefault(m => m.Uuid == selectedMap)?.AgentDisplayName = "-";
                radioButton.IsChecked = false;
            }
            else
            {
                selectedAgents[selectedMap] = agentButton.Uuid;
                labelSelectedMap2.Content = agentButton.DisplayName;
                mapsCollection.FirstOrDefault(m => m.Uuid == selectedMap)?.AgentDisplayName = agentButton.DisplayName;
            }
            labelStatus.Content = $"Clicked: {agentButton.DisplayName}";
        }
    }

    private void MapDropdown_Click(object sender, RoutedEventArgs e)
    {
        MapDropdownPopup.IsOpen = !MapDropdownPopup.IsOpen;
    }

    private void MapDropdownItem_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.DataContext is MapButton mapButton)
        {
            labelSelectedMap.Content = mapButton.DisplayName;
            labelSelectedMap2.Content = mapButton.AgentDisplayName;

            if (selectedAgents.TryGetValue(mapButton.Uuid, out string? agentId) && !string.IsNullOrEmpty(agentId))
            {
                agentsCollection.FirstOrDefault(a => a.Uuid == agentId)?.IsChecked = true;
            }
            else if (selectedAgents.TryGetValue(selectedMap, out string? oldAgentId) && !string.IsNullOrEmpty(oldAgentId))
            {
                agentsCollection.FirstOrDefault(a => a.Uuid == oldAgentId)?.IsChecked = false;
            }

            selectedMap = mapButton.Uuid;
        }
    }
}