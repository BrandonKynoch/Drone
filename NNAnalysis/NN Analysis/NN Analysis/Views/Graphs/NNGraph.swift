//
//  NNGraph.swift
//  Neural Analysis
//
//  Created by Brandon Kynoch on 2022/07/14.
//

import SwiftUI

struct NNGraph: View {
    @ObservedObject var nnGroup: NNGroup
    @State var i: Int = 0
    
    var body: some View {
        if let currentViewingNN = nnGroup.currentViewingNN {
            ZStack {
                BackgroundView(viewMode: .dark, opacity: 0.5)
                VStack {
                    // Neural shape
                    ZStack {
                        PanelSolidView(colour: COL_BACKGROUND2)
                        
                        HStack {
                            ForEach(currentViewingNN.UICountArray, id: \.self) { i in
                                Spacer()
                                VStack {
                                    Text("\(currentViewingNN.neuralShape[i])")
                                        .modifier(BodyTextModifier())
                                        .foregroundColor(COL_TEXT)
                                    if i-1 >= 0 {
                                        Text("\(currentViewingNN.GetActivationName(atIndex: i-1))")
                                            .modifier(BodyTextModifier())
                                            .foregroundColor(COL_TEXT)
                                    } else {
                                        Spacer()
                                    }
                                }
                                Spacer()
                            }
                        }
                        .padding()
                    }
                    .frame(height: 50)
                    .padding()
                    
                    ZStack {
                        WeightView(nn: currentViewingNN)
                        BiasView(nn: currentViewingNN)
                    }
                    .padding()
                }
            }.cornerRadius(CORNER_RADIUS)
        }
    }
}

struct BiasView: View {
    let nn: NN
    
    var body: some View {
        GeometryReader { geometry in
            let frameHeight = geometry.size.height
            let frameWidth = geometry.size.width
            
            let layerSpacing = frameWidth / CGFloat(nn.neuralSize)
            let xAxisScale = 2.5 * (frameWidth / 500)

            ForEach(nn.UIIndexArray, id: \.self) { i in
                let baseX = layerSpacing * (CGFloat(i) + 1.5)
                let ySegmentSpacing = frameHeight / CGFloat(nn.maxShape)
                let baseY = (frameHeight / 2) - ((CGFloat(nn.layers[i].biases.count) / 2) * ySegmentSpacing)
                Path { path in
                    path.move(to: CGPoint(
                        x: baseX,
                        y: baseY
                    ))
    
                    for biasI in 1..<nn.layers[i].biases.count {
                        path.addLine(to: CGPoint(
                            x: baseX + (nn.layers[i].biases[biasI] * xAxisScale),
                            y: baseY + CGFloat(biasI) * ySegmentSpacing
                        ))
                    }
                }
                .stroke(
                    COL_ACC1,
                    style: StrokeStyle(lineWidth: 3, lineCap: .round, lineJoin: .round)
                )
                
                // Line down the middle
                Path { path in
                    path.move(to: CGPoint(
                        x: baseX,
                        y: baseY
                    ))
                    
                    path.addLine(to: CGPoint(x: baseX, y: baseY + CGFloat(nn.layers[i].biases.count - 1) * ySegmentSpacing))
                }
                .stroke(
                    COL_ACC1,
                    style: StrokeStyle(lineWidth: 2, lineCap: .round, lineJoin: .round, miterLimit: 3, dash: [10], dashPhase: 0)
                )
            }
        }
    }
}

struct WeightView: View {
    let nn: NN
    
    var body: some View {
        ZStack {
            GeometryReader { geometry in
                let frameHeight = geometry.size.height
                let frameWidth = geometry.size.width
                
                let layerSpacing = frameWidth / CGFloat(nn.neuralSize)

                ForEach(nn.UIIndexArray, id: \.self) { i in
                    SubView(
                        nnLayer: nn.layers[i],
                        maxShape: nn.maxShape,
                        i: i,
                        layerSpacing: layerSpacing,
                        frameHeight: frameHeight,
                        frameWidth: frameWidth
                    )
                }
            }
        }
        .drawingGroup()
    }
    
    // View is too complex to represent in a single view
    @ViewBuilder
    func SubView(nnLayer: NN.NNLayer, maxShape: Int, i: Int, layerSpacing: CGFloat, frameHeight: CGFloat, frameWidth: CGFloat) -> some View {
        let baseXLeft = layerSpacing * (CGFloat(i) + 0.5)
        let baseXRight = layerSpacing * (CGFloat(i + 1) + 0.5)
        let ySegmentSpacing = frameHeight / CGFloat(maxShape)
        let baseYLeft : CGFloat = (frameHeight / 2) - ((CGFloat(nnLayer.weights.count) / 2) * ySegmentSpacing * CGFloat(nnLayer.colsGrouping))
        let baseYRight : CGFloat = (frameHeight / 2) - ((CGFloat(nnLayer.weights[0].count) / 2) * ySegmentSpacing * CGFloat(nnLayer.rowsGrouping))
        
        WeightLayerView(
            nnLayer: nn.layers[i],
            baseXLeft: baseXLeft,
            baseXRight: baseXRight,
            baseYLeft: baseYLeft,
            baseYRight: baseYRight,
            ySegmentSpacing: ySegmentSpacing,
            maxWeightMagnitude: CGFloat(nn.maximumWeightMagnitude)
        )
    }
}


struct WeightLayerView: View {
    @EnvironmentObject var analysisHandler: AnalysisHandler
    
    let nnLayer: NN.NNLayer
    
    let baseXLeft: CGFloat
    let baseXRight: CGFloat
    let baseYLeft: CGFloat
    let baseYRight: CGFloat
    let ySegmentSpacing: CGFloat
    
    let maxWeightMagnitude: CGFloat
    
    var body: some View {
        ForEach (nnLayer.UIReduceColsArray, id: \.self) { j in
            ForEach (nnLayer.UIReduceRowsArray, id: \.self) { k in
                Path { path in
                        path.move(to: CGPoint(
                            x: baseXLeft,
                            y: baseYLeft + (CGFloat(j) * ySegmentSpacing * CGFloat(nnLayer.colsGrouping))
                        ))
                        
                        path.addLine(to: CGPoint(
                            x: baseXRight,
                            y: baseYRight + (CGFloat(k) * ySegmentSpacing * CGFloat(nnLayer.rowsGrouping))
                        ))
                }
                .stroke(
                    COL_ACC2.opacity((nnLayer.weights[j][k] / maxWeightMagnitude) * analysisHandler.weightOpacityScaler),
                    style: StrokeStyle(lineWidth: 3, lineCap: .round, lineJoin: .round)
                )
            }
        }
    }
}
