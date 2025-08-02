export const tableStyles = {
  container: 'p-4',
  title: 'text-2xl font-bold mb-4',
  infoBar: 'mb-2 text-sm text-gray-600 flex items-center gap-4',
  editableIndicator: 'text-blue-600',
  loadingContainer: 'flex items-center',
  spinner:
    'animate-spin h-4 w-4 border-2 border-gray-500 border-t-transparent rounded-full mr-2',
  allDataLoaded: 'text-gray-500',
  gridContainer: {
    height: '600px',
    width: '100%',
  },
  noDataContainer: 'flex items-center justify-center h-64 text-gray-500',
  rightPadding: 'pr-2',
  footerText: 'mt-4 text-sm text-gray-500',
  errorContainer: 'p-4',
  errorText: 'text-red-600 mb-4',
  retryButton: 'px-4 py-2 bg-blue-500 text-white rounded hover:bg-blue-600',
} as const;
