//
//  NN.swift
//  Neural Analysis
//
//  Created by Brandon Kynoch on 7/14/22.
//

import Foundation

class NN: ObservableObject {
    public let name: String
    
    private(set) var neuralSize: Int = 0
    private(set) var neuralShape = [Int]()
    private(set) var activations = [Int]()
    private(set) var layers = [NNLayer]()
    
    private(set) var maxShape: Int = 0
    
    private(set) var maximumWeightMagnitude: Double = 1
    
    // UI VARS ///////////////////////////////////    
    private(set) var UIIndexArray = [Int]() // Used for foreach loops in UI
    private(set) var UICountArray = [Int]() // Used for foreach loops in UI
    // UI VARS ///////////////////////////////////
    
    init(fromFile: URL) throws {
        name = fromFile.lastPathComponent
        
        let file = try FileHandle(forReadingFrom: fromFile)
        
        let fileData: Data = try file.readToEnd()!

        fileData.withUnsafeBytes({ pointer in
            var index = 0

            self.neuralSize = Int(pointer.ReadBytes(Int32.self, offset: &index))
            
            for i in 0..<neuralSize {
                let shape = Int(pointer.ReadBytes(Int32.self, offset: &index))
                if shape > maxShape {
                    maxShape = shape
                }
                neuralShape.append(shape)
                UICountArray.append(i)
            }
            
            for i in 0..<(neuralSize - 1) {
                activations.append(Int(pointer.ReadBytes(Int32.self, offset: &index)))
                UIIndexArray.append(i)
            }
            
            for i in 0..<(neuralSize - 1) {
                layers.append(NNLayer(
                    pointer: pointer,
                    shapeLeft: neuralShape[i],
                    shapeRight: neuralShape[i+1],
                    index: &index)
                )
            }
            
            self.FindMaximumWeight()
        })
    }
    
    private func FindMaximumWeight() {
        for layer in layers {
            layer.FindMaximumWeight()
        }
        
        maximumWeightMagnitude = 0
        
        for layer in layers {
            if layer.maximumWeightMagnitude! > maximumWeightMagnitude {
                maximumWeightMagnitude = layer.maximumWeightMagnitude!
            }
        }
        
        if maximumWeightMagnitude == 0 {
            maximumWeightMagnitude = 1
        }
    }
    
    public func GetActivationName(atIndex: Int) -> String {
        switch activations[atIndex] {
        case 0:
            return "Relu"
        case 1:
            return "Leaky Relu"
        case 2:
            return "Sigmoid"
        default:
            return "Unknown"
        }
    }
    
    
    
    // Represents the weights and biases used for transitioning from layer i to (i+1)
    class NNLayer {
        var originalLayerSizeLeft: Int // Number of cols
        let originalLayerSizeRight: Int // Number of rows
        var weights = [[Double]]()
        var biases = [Double]()
        
        private(set) var maximumWeightMagnitude: Double? = nil
        
        // Grouping measures how many values have been reduced into a single element
        private(set) var rowsGrouping: Int = 1 // For matrix reduction
        private(set) var colsGrouping: Int = 1 // For matrix reduction
        
        // Index arrays just used for drawing UI
        private(set) var UIRowsArray = [Int]() // Size of original matrix
        private(set) var UIColsArray = [Int]() // Size of original matrix
        
        private(set) var UIReduceRowsArray = [Int]()
        private(set) var UIReduceColsArray = [Int]()
        // Index arrays just used for drawing UI
        
        // CONSTANTS //////////////////////////////////
        private let MAX_LAYER_SIZE = 20
        // CONSTANTS //////////////////////////////////
        
        init(pointer: UnsafeRawBufferPointer, shapeLeft: Int, shapeRight: Int, index: inout Int) {
            originalLayerSizeLeft = shapeLeft
            originalLayerSizeRight = shapeRight
            
            // shapeLeft = number of columns
            // shapeRight = number of rows
            for _ in 0..<shapeLeft {
                weights.append([Double]())
            }
            
            // Using row major order
            for i in 0..<shapeLeft { // Columns
                for _ in 0..<shapeRight { // Rows
                    weights[i].append(pointer.ReadBytes(Double.self, offset: &index))
                }
                UIColsArray.append(i)
            }
            
            for i in 0..<shapeRight {
                biases.append(pointer.ReadBytes(Double.self, offset: &index))
                UIRowsArray.append(i)
            }
            
            UIReduceColsArray = UIColsArray
            UIReduceRowsArray = UIRowsArray
            
            // NOTE: FindMaximumWeight is called inside ReduceLayerSize
            self.ReduceLayerSize()
        }
        
        func ReduceLayerSize() {
            var tmpWeights = [[Double]]()
            
            colsGrouping = (weights.count > MAX_LAYER_SIZE) ? (Int(Double(weights.count) / Double(MAX_LAYER_SIZE)) + 1): 1
            rowsGrouping = (weights[0].count > MAX_LAYER_SIZE) ? (Int(Double(weights[0].count) / Double(MAX_LAYER_SIZE)) + 1): 1
            let kernelSize = Double(rowsGrouping * colsGrouping)
            
            UIReduceColsArray = [Int]()
            UIReduceRowsArray = [Int]()
            
            for i in 0..<(originalLayerSizeLeft / colsGrouping) {
                tmpWeights.append([Double]())
                UIReduceColsArray.append(i)
                for j in 0..<(originalLayerSizeRight / rowsGrouping) {
                    let iIn = i * colsGrouping
                    let jIn = j * rowsGrouping
                    var sum : Double = 0
                    for k in 0..<colsGrouping {
                        for l in 0..<rowsGrouping {
                            sum += weights[iIn + k][jIn + l]
                        }
                    }
                    tmpWeights[i].append(sum / kernelSize)
                }
            }
            
            for i in 0..<(originalLayerSizeRight / rowsGrouping) {
                UIReduceRowsArray.append(i)
            }
            
            self.weights = tmpWeights
            
            FindMaximumWeight()
        }
        
        public func FindMaximumWeight() {
            guard maximumWeightMagnitude == nil else {
                return
            }
            
            maximumWeightMagnitude = 0
            
            for a in weights { // Cols
                for d in a { // Rows
                    if d > maximumWeightMagnitude! {
                        maximumWeightMagnitude = d
                    }
                }
            }
            
            if maximumWeightMagnitude == 0 {
                maximumWeightMagnitude = 1
            }
        }
    }
}
