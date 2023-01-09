import React, { ChangeEvent, FormEvent, useState } from 'react';
import logo from './logo.svg';
import './App.css';

function App() {
  const [imageToUpload, setImageToUpload] = useState<File | null>(null);
  const [responseImage, setResponseImage] = useState<string>();
  const [chunkSize, setChunkSize] = useState<string>('50');
  const [colorsAmount, setColorsAmount] = useState<string>('1000');
  const [reduceColorsAmount, setReduceColorsAmount] = useState<boolean>(false);

  const [isLoading, setIsLoading] = useState<boolean>(false);

  const handleChunkSizeChange = (e: ChangeEvent<HTMLInputElement>) => {
    const value = Number(e.target.value)
    if (Number.isNaN(value)) {
      return
    }
    if (value < 1) {
      setChunkSize('')
      return
    }

    setChunkSize(value.toString())
  }

  const handleColorsAmountChange = (e: ChangeEvent<HTMLInputElement>) => {
    const value = Number(e.target.value)
    if (Number.isNaN(value)) {
      return
    }
    if (value < 1) {
      setColorsAmount('')
      return
    }

    setColorsAmount(value.toString())
  }

  const handleReduceColorsChange = (_e: ChangeEvent<HTMLInputElement>) => {
    setReduceColorsAmount(v => !v);
  }

  function handleImageChange(e: ChangeEvent<HTMLInputElement>) {
    if (!e.target.files?.length) {
      alert('no files')
      return
    }

    setImageToUpload(e.target.files[0]);
  }

  function handleSubmit(e: FormEvent<HTMLFormElement>) {
    e.preventDefault();
    if (!imageToUpload) return alert('Upload image')
    if (!chunkSize) return alert('Enter the chunk size')
    if (!colorsAmount && reduceColorsAmount) return alert('Enter the colors amount')

    setIsLoading(true)
    const formData = new FormData();
    formData.append('image', imageToUpload);
    formData.append('chunkSize', chunkSize)
    if (reduceColorsAmount) {
      formData.append('colorsAmount', colorsAmount)
    }
    fetch('http://localhost:5168/Image', {
      method: 'POST',
      body: formData
    })
    .then(response => response.json())
    .then(response => {
      setResponseImage("data:image/png;base64," + response.data);
    })
    .catch(e => {
      alert(e);
    })
    .finally(() => {
      setIsLoading(false)
    })
  }
  
  return (
    <>
      {isLoading && <div className='App-logo loading-indicator'/>}
      <div className="App">
        <form onSubmit={handleSubmit} className="form">
          <input type="file" accept="image/*" onChange={handleImageChange} />
          <label>
            Chunk size
            <input type="number" value={chunkSize} onChange={handleChunkSizeChange} />
          </label>
          <label>
            Reduce colors
            <input type="checkbox" checked={reduceColorsAmount} onChange={handleReduceColorsChange} />
          </label>
          <label>
            Reduce colors amount
            <input type="number" value={colorsAmount} disabled={!reduceColorsAmount} onChange={handleColorsAmountChange} />
          </label>
          <button type="submit">Upload</button>
        </form>
        <div className="images">
          {responseImage && <img src={responseImage} alt="Response Image" />}
          {imageToUpload && <img src={URL.createObjectURL(imageToUpload)} alt="Uploaded Image" />}
        </div>
      </div>
    </>
  );
}

export default App;
