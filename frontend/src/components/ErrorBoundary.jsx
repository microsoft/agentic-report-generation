import React from 'react';
import sadPuppy from '../assets/sad-puppy.jpg';

export default class ErrorBoundary extends React.Component {
  constructor(props) {
    super(props);
    this.state = { hasError: false };
  }
  static getDerivedStateFromError() {
    return { hasError: true };
  }
  handleRefresh = () => {
    if (this.props.onRefresh) {
      this.props.onRefresh();
      this.setState({ hasError: false });
    } else {
      window.location.reload();
    }
  };
  render() {
    if (this.state.hasError) {
      return (
        <div className="p-4 flex flex-col">
          <h2 className="text-2xl font-semibold mb-4">Something went wrong.</h2> 
          <img
            src={sadPuppy}
            alt="Sad Puppy"
            className="w-64 h-64 mb-4"
          />
          <button
            onClick={this.handleRefresh}
            className="bg-blue-500 text-white px-4 py-2 rounded hover:bg-blue-700"
          >
            Refresh
          </button>
        </div>
      );
    }
    return this.props.children;
  }
}
